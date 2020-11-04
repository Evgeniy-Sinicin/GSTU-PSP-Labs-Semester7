using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace LAB_5_SOLAE_HTTP_SERVER_CS
{
    public class Client
    {
        private TcpClient _client;

        private void SendError(int code)
        {
            var statusCode = $"{code} {((HttpStatusCode)code)}";
            var html = $"<html><body><h1>{statusCode}</h1><form action=\"/\" method=\"GET\"><input type=\"submit\" value=\"Home\"></form></body></html>";
            var headers = $"HTTP/1.1 {statusCode}\nContent-Type: text/html\nContent-Length: {html.Length}\n\n";
            var message = $"{headers}{html}";
            var buffer = Encoding.UTF8.GetBytes(message);

            _client.GetStream().Write(buffer, 0, buffer.Length);
            _client.Close();
        }

        public Client(TcpClient client)
        {
            _client = client;

            var request = string.Empty;
            var buffer = new byte[1024];

            while (_client.GetStream().Read(buffer, 0, buffer.Length) > 0)
            {
                request += Encoding.UTF8.GetString(buffer, 0, buffer.Length);

                if (request.IndexOf("\r\n\r\n") >= 0 ||
                    request.Length > 4096)
                {
                    break;
                }
            }

            Console.WriteLine(request);

            Match match = Regex.Match(request, @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");

            if (match == Match.Empty)
            {
                SendError(400);
                return;
            }

            var uri = Uri.UnescapeDataString(match.Groups[1].Value);

            if (uri.IndexOf("..") >= 0)
            {
                SendError(400);
                return;
            }

            if (uri.EndsWith("/"))
            {
                uri += "index.html";
            }

            var path = $"../../www/{uri}";

            if (uri.Equals("/matrix.html"))
            {
                SendMatrixPage(match.Value);
                return;
            }
            else if (uri.Equals("/result.html"))
            {
                SendResultPage(request);
                return;
            }
            else if (!File.Exists(path))
            {
                SendError(404);
                return;
            }

            var extension = uri.Substring(uri.LastIndexOf('.'));
            var contentType = string.Empty;

            switch (extension)
            {
                case ".htm":
                case ".html":
                {
                    contentType = "text/html";
                    break;
                }
                case ".css":
                {
                    contentType = "text/stylesheet";
                    break;
                }
                case ".js":
                {
                    contentType = "text/javascript";
                    break;
                }
                case ".jpg":
                {
                    contentType = "image/jpeg";
                    break;
                }
                case ".jpeg":
                case ".png":
                case ".gif":
                {
                    contentType = $"image/{extension.Substring(1)}";
                    break;
                }
                default:
                if (extension.Length > 1)
                {
                    contentType = $"application/{extension.Substring(1)}";
                }
                else
                {
                    contentType = "application/unknown";
                }
                break;
            }

            FileStream fs;

            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception)
            {
                SendError(500);
                return;
            }

            var statusCode = "200 OK";
            var headers = $"HTTP/1.1 {statusCode}\nContent-Type: {contentType}\nContent-Length: {fs.Length}\n\n";
            var headersBuffer = Encoding.UTF8.GetBytes(headers);

            _client.GetStream().Write(headersBuffer, 0, headersBuffer.Length);

            while (fs.Position < fs.Length)
            {
                fs.Read(buffer, 0, buffer.Length);
                client.GetStream().Write(buffer, 0, buffer.Length);
            }

            fs.Close();
            _client.Close();
        }

        private void SendResultPage(string request)
        {
            var separator = "\r\n\r\n";
            var startPos = request.IndexOf(separator) + separator.Length;
            var parameters = request.Substring(startPos);
            parameters = parameters.Remove(parameters.IndexOf('\0'));
            var paramsList = GetUrlParams(parameters);
            var size = int.Parse(paramsList.FirstOrDefault(x => x[0].Equals("Size"))[1]);
            var system = DenseMatrix.OfArray(new double[size, size]) as Matrix<double>;
            var coeffs = DenseMatrix.OfArray(new double[size, 1]) as Matrix<double>;
            var sysVals = paramsList.Where(x => x[0].Contains("MatrixValue")).Select(x => x[1]).ToList();
            var coeffsVals = paramsList.Where(x => x[0].Contains("CoeffsValue")).Select(x => x[1]).ToList();

            try
            {
                for (int i = 0; i < size; i++)
                {
                    coeffs[i, 0] = double.Parse(coeffsVals[i]);

                    for (int j = 0; j < size; j++)
                    {
                        system[i, j] = double.Parse(sysVals[i * size + j]);
                    }
                }
            }
            catch (Exception)
            {
                SendError(400);
                return;
            }

            var decision = system.Inverse() * coeffs;

            var statusCode = $"200 OK";
            var html = $"<html><body><h1>SOLAE Decision</h1>";
            for (int i = 0; i < size; i++)
            {
                html += $"<h2>Decision[{i}]: {decision[i, 0]}</h2>";
            }
            html += "<form action=\"/\" method=\"GET\"><input type=\"submit\" value=\"Home\"></form></body></html>";

            var headers = $"HTTP/1.1 {statusCode}\nContent-Type: text/html\nContent-Length: {html.Length}\n\n";
            var message = $"{headers}{html}";
            var buffer = Encoding.UTF8.GetBytes(message);

            _client.GetStream().Write(buffer, 0, buffer.Length);
            _client.Close();
        }

        private void SendMatrixPage(string url)
        {
            var parameters = GetUrlParams(url.Split(' ')[1]);
            var size = 0;

            try
            {
                size = int.Parse(parameters[0][1]);
            }
            catch (Exception)
            {
                SendError(400);
                return;
            }
            
            var statusCode = $"200 OK";
            var html = $"<html><body><h1>Matrix Page</h1><form action=\"result.html\" method=\"POST\">";

            html += "<div>";
            html += $"<span><p>Size: {size}<input type=\"hidden\" name=\"Size\" value=\"{size}\"></p></span>";
            for (int i = 0; i < size; i++)
            {
                html += "<span style=\"float: left\">";
                for (int j = 0; j < size; j++)
                {
                    html += $"<p style=\"float: left\">System[{i * size + j}]: <input type=\"text\" name=\"MatrixValue{i * size + j}\"></p>";
                }
                html += "</span>";
            }
            html += "</div></br></br>";

            for (int i = 0; i < size; i++)
            {
                html += "</br></br>";
            }

            html += "<div>";
            for (int i = 0; i < size; i++)
            {
                html += $"<p>Coeffs[{i}]: <input type=\"text\" name=\"CoeffsValue{i}\"></p>";
            }
            html += "</div>";

            html += "<input type=\"submit\" value=\"Calculate\"></form></body></html>";

            var headers = $"HTTP/1.1 {statusCode}\nContent-Type: text/html\nContent-Length: {html.Length}\n\n";
            var message = $"{headers}{html}";
            var buffer = Encoding.UTF8.GetBytes(message);

            _client.GetStream().Write(buffer, 0, buffer.Length);
            _client.Close();
        }

        private List<string[]> GetUrlParams(string url)
        {
            var prms = new List<string[]>();
            var prmsLines = url.Remove(0, url.IndexOf('?') + 1).Split('&');
            
            prmsLines.ToList().ForEach(x => prms.Add(new string[2] { x.Split('=')[0], x.Split('=')[1] }));

            return prms;
        }
    }
}
