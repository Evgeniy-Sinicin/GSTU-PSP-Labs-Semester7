using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;

namespace LAB_7_CHAT_WEBSOCKETS_SERVER_ASP_MVC
{
    /// <summary>
    /// Сводное описание для ChatHandler
    /// </summary>
    public class ChatHandler : IHttpHandler
    {
        private static readonly List<WebSocket> _clients = new List<WebSocket>();

        /// <summary>
        /// Блокировщик для обеспечения потокобезопасности
        /// </summary>
        private static readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        /// <summary>
        /// Метод обработки входящих запросов
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            // Если запрос от веб-сокета
            if (context.IsWebSocketRequest)
            {
                // Принимает и обрабатываем его с помощью нашего метода
                context.AcceptWebSocketRequest(WebSocketRequest);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private async Task WebSocketRequest(AspNetWebSocketContext context)
        {
            var client = context.WebSocket;

            // Пытаемся безопасно добавить клиента в список
            _locker.EnterWriteLock();
            try
            {
                _clients.Add(client);
            }
            finally
            {
                _locker.ExitWriteLock();
            }

            // Прослушка клиента
            while (true)
            {
                var buffer = new ArraySegment<byte>(new byte[1024]);

                // Получаем сообщение от клиента
                var result = await client.ReceiveAsync(buffer, CancellationToken.None);


                // Отправляем сообщение остальным клиентам
                for (int i = 0; i < _clients.Count; i++)
                {
                    var otherClient = _clients[i];

                    try
                    {
                        // Если с другим клиентом установлено соединение
                        if (otherClient.State == WebSocketState.Open)
                        {
                            // Ожидаем пока завершится поток отправки сообщения
                            await otherClient.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                    // Если соединение с другим клиентом оборвалось
                    catch (ObjectDisposedException)
                    {
                        // Безопасно удаляем другого клиента из списка клиентов
                        _locker.EnterWriteLock();
                        try
                        {
                            _clients.Remove(otherClient);
                            i--;
                        }
                        finally
                        {
                            _locker.ExitWriteLock();
                        }
                    }
                }
            }
        }
    }
}