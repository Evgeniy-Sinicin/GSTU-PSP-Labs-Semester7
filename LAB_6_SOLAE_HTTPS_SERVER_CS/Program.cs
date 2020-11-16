using LAB_6_SOLAE_HTTPS_SERVER_CS.Dtos;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace LAB_6_SOLAE_HTTPS_SERVER_CS
{
    public class Program
    {
        //private static X509Certificate _serverCerf = null;
        private static X509Certificate2 _serverCerf = null;

        private static void Main(string[] args)
        {
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //_serverCerf = X509Certificate.CreateFromSignedFile("chrome.cer"); //192.168.42.38
            _serverCerf = new X509Certificate2("cert.pfx", "1234567890"); //192.168.42.38

            var ip = Dns.GetHostAddresses(Dns.GetHostName())[Dns.GetHostAddresses(Dns.GetHostName()).Length - 1];
            var port = 443; //27015; //443; //8181; //443; //443; //44315; //443;

            Console.WriteLine($"Server starts by link: https://{ip}:{port}/index.html\n");

            var listener = new TcpListener(ip, port);

            listener.Start();

            while (true)
            {
                var client = listener.AcceptTcpClient();

                ThreadPool.QueueUserWorkItem(ProcessClient, client);
            }
        }

        private static string GetHeaders(int contentLength, int code = 200)
        {
            var statusCode = $"{code} {((HttpStatusCode)code)}";
            var headers = new StringBuilder();

            headers.Append($"HTTP/1.1 {statusCode}").Append("\r\n");
            headers.Append("Date: ").Append(DateTime.Now).Append("\r\n");
            headers.Append("Server: nesterione-server").Append("\r\n");
            headers.Append("Content-Type: text/html; charset=UTF-8").Append("\r\n");
            headers.Append("Content-Length: ").Append(contentLength).Append("\r\n");
            headers.Append("Connection: close").Append("\r\n");
            headers.Append("\r\n");

            return headers.ToString();
        }

        private static string GetResultPage(string request)
        {
            var separator = "\r\n\r\n";
            var startPos = request.IndexOf(separator) + separator.Length;
            var json = request.Substring(startPos);

            var jss = new JavaScriptSerializer();
            var message = jss.Deserialize<Message>(json);

            var size = message.Decisions.Count;
            var system = DenseMatrix.OfArray(new double[size, size]) as Matrix<double>;
            var coeffs = DenseMatrix.OfArray(new double[size, 1]) as Matrix<double>;

            for (int i = 0; i < size; i++)
            {
                coeffs[i, 0] = message.Coeffs[i];

                for (int j = 0; j < size; j++)
                {
                    system[i, j] = message.Matrix[i * size + j];
                }
            }

            var decision = system.Inverse() * coeffs;

            for (int i = 0; i < size; i++)
            {
                message.Decisions[i] = decision[i, 0];
            }

            json = jss.Serialize(message);

            //var size = int.Parse(parameters.FirstOrDefault(x => x[0].Equals("Size"))[1]);
            //var system = DenseMatrix.OfArray(new double[size, size]) as Matrix<double>;
            //var coeffs = DenseMatrix.OfArray(new double[size, 1]) as Matrix<double>;
            //var sysVals = parameters.Where(x => x[0].Contains("MatrixValue")).Select(x => x[1]).ToList();
            //var coeffsVals = parameters.Where(x => x[0].Contains("CoeffsValue")).Select(x => x[1]).ToList();

            //try
            //{
            //    for (int i = 0; i < size; i++)
            //    {
            //        coeffs[i, 0] = double.Parse(coeffsVals[i]);

            //        for (int j = 0; j < size; j++)
            //        {
            //            system[i, j] = double.Parse(sysVals[i * size + j]);
            //        }
            //    }
            //}
            //catch (Exception)
            //{
            //    return GetBadAnswerPage(400);
            //}

            //var decision = system.Inverse() * coeffs;
            //var html = $"<html><body><h1>SOLAE Decision</h1>";

            //for (int i = 0; i < size; i++)
            //{
            //    html += $"<h2>Decision[{i}]: {decision[i, 0]}</h2>";
            //}

            //html += "<h3><a href='/'>Open Index Page</a></h3></body></html>";

            return string.Concat(GetHeaders(json.Length), json);
        }

        private static string GetBadAnswerPage(int code)
        {
            var statusCode = $"{code} {((HttpStatusCode)code)}";
            var html = new StringBuilder();

            html.Append("<html><body>");
            html.Append("<h1>");
            html.Append(statusCode);
            html.Append("</h1>");
            html.Append("</h2>");
            html.Append("<a href='/'>Open Index Page</a>");
            html.Append("</h2>");
            html.Append("</body></html>");

            return string.Concat(GetHeaders(html.Length, code), html);
        }

        private static string GetIndexPage()
        {
            var body = File.ReadAllText($"../../www/index.html");

            //var sb = new StringBuilder();

            //sb.Append("<html>");
            //sb.Append("<body>");
            //sb.Append("<h1>");
            //sb.Append("Enter SOLAE Form");
            //sb.Append("</h1>");
            //sb.Append("<form action='matrix.html' method='GET'>");
            //sb.Append("<p>");
            //sb.Append("Matrix Size: ");
            //sb.Append("<input type='text' name='matrixSize'>");
            //sb.Append("</p>");
            //sb.Append("<input type='submit' value='Init'>");
            //sb.Append("</form>");
            //sb.Append("</body>");
            //sb.Append("</html>");

            //var body = sb.ToString();

            return string.Concat(GetHeaders(body.Length), body);
        }

        private static string GetMatrixPage(int size)
        {
            var html = File.ReadAllText($"../../www/matrix.html");

            //var html = $"<html><body><h1>Matrix Page</h1><form action=\"result.html\" method=\"POST\">";

            //html += "<div>";
            //html += $"<span><p>Size: {size}<input type=\"hidden\" name=\"Size\" value=\"{size}\"></p></span>";
            //for (int i = 0; i < size; i++)
            //{
            //    html += "<span style=\"float: left\">";
            //    for (int j = 0; j < size; j++)
            //    {
            //        html += $"<p style=\"float: left\">System[{i * size + j}]: <input type=\"text\" name=\"MatrixValue{i * size + j}\"></p>";
            //    }
            //    html += "</span>";
            //}
            //html += "</div></br></br>";

            //for (int i = 0; i < size; i++)
            //{
            //    html += "</br></br>";
            //}

            //html += "<div>";
            //for (int i = 0; i < size; i++)
            //{
            //    html += $"<p>Coeffs[{i}]: <input type=\"text\" name=\"CoeffsValue{i}\"></p>";
            //}
            //html += "</div>";

            //html += "<input type=\"submit\" value=\"Calculate\"></form></body></html>";

            return string.Concat(GetHeaders(html.Length), html);
        }

        private static string ReadMessage(SslStream ssl)
        {
            var sb = new StringBuilder();

            try
            {
                var buffer = new byte[2048];
                var bytes = -1;

                do
                {
                    bytes = ssl.Read(buffer, 0, buffer.Length);

                    var decoder = Encoding.UTF8.GetDecoder();
                    var chars = new char[decoder.GetCharCount(buffer, 0, bytes)];

                    decoder.GetChars(buffer, 0, bytes, chars, 0);
                    sb.Append(chars);
                }
                while (bytes != 0);
            }
            catch (Exception)
            {
                Console.WriteLine("Error in ReadMessage!");
            }

            return sb.ToString();
        }

        private static List<string[]> GetUrlParams(string url)
        {
            var prms = new List<string[]>();
            var prmsLines = url.Remove(0, url.IndexOf('?') + 1).Split('&');

            prmsLines.ToList().ForEach(x => prms.Add(new string[2] { x.Split('=')[0], x.Split('=')[1] }));

            return prms;
        }

        private static void ProcessClient(object obj)
        {
            var client = (TcpClient)obj;
            var ssl = new SslStream(client.GetStream(), false);

            try 
            {
                var timeout = 5000;

                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                ssl.AuthenticateAsServer(_serverCerf, false, SslProtocols.Ssl3 | SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls, true);
                ssl.ReadTimeout = timeout;
                ssl.WriteTimeout = timeout;

                Console.WriteLine("Waiting for client message...");

                var data = ReadMessage(ssl);

                Console.WriteLine($"Request:\n{data}\n");

                var page = string.Empty;
                var match = Regex.Match(data, @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");

                if (match == Match.Empty)
                {
                    page = GetBadAnswerPage(400);
                }
                else
                {
                    var uri = Uri.UnescapeDataString(match.Groups[1].Value);

                    if (uri.IndexOf("..") >= 0)
                    {
                        page = GetBadAnswerPage(400);
                    }
                    else
                    {
                        var reg = new Regex("\r\n\r\n");
                        var request = reg.Split(data, 2);

                        if (uri.Equals("/"))
                        {
                            uri += "index.html";
                            page = GetIndexPage();
                        }
                        else if (uri.Equals("/index.html"))
                        {
                            page = GetIndexPage();
                        }
                        else if (data.StartsWith("GET") && 
                                uri.Equals("/matrix.html"))
                        {
                            var parameter = GetUrlParams(match.Value.Split(' ')[1])[0];
                            var size = 0;

                            if (int.TryParse(parameter[1], out size))
                            {
                                page = GetMatrixPage(size);
                            }
                            else
                            {
                                page = GetBadAnswerPage(400);
                            }
                        }
                        else if (data.StartsWith("POST") && 
                                uri.Equals("/matrix.html"))
                        {
                            page = GetResultPage(data);
                        }
                        else
                        {
                            page = GetBadAnswerPage(404);
                        }
                    }
                }

                Console.WriteLine($"Response:\n{page}\n");

                var message = Encoding.UTF8.GetBytes(page);

                ssl.Write(message, 0, message.Length);
                ssl.Flush();
            }
            catch (/*AuthenticationException*/ Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}!");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                Console.WriteLine("Authentication failed - closing the connection...");
            }
            finally
            {
                ssl.Close();
                client.Close();
            }
        }
    }
}
