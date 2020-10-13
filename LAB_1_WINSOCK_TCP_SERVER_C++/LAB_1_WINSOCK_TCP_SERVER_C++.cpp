#pragma region Test 1
#include <winsock2.h>
#include <ws2tcpip.h>
#include <stdio.h>
#include <typeinfo>
#include <iphlpapi.h>
#include <iostream>

#pragma comment(lib, "Ws2_32.lib")
#pragma comment(lib, "iphlpapi.lib")

#define DEFAULT_BUFFER_LENGTH 512
#define SERVER_PORT "27015"

DWORD WINAPI clientHandler(LPVOID clientSocket);

bool _finishedWork;

int _onlineClientCount;
int _maxOnlineClientCount;
int _readyClientCount;
int _requiredClientCount;
int _initialInterval;
int _finalInterval;
int _precision;

float _commonSquare;
double _pcfReq;
INT64 _counterStart;

struct Message
{
	const int client_id;
	const float initial_interval;
	const float final_interval;
	const int precision;
	float square;
};

int main(int argc, char* argv[])
{
	printf("Server started to work...\n\n");
	printf("Enter required client count: ");
	scanf("%d", &_requiredClientCount);
	printf("Enter initial interval: ");
	scanf("%d", &_initialInterval);
	printf("Enter final interval: ");
	scanf("%d", &_finalInterval);
	printf("Enter required precision: ");
	scanf("%d", &_precision);

	int funcResult;
	WSADATA wsaData;

	funcResult = WSAStartup(MAKEWORD(2, 2), &wsaData);

	if (funcResult)
	{
		printf("Error #%d: WSAStartup failed!\n", funcResult);
	}

	struct addrinfo* result = NULL, // Требуется для получения информации об адресе сокета
				   * ptr = NULL,    // Требуется для хранения информации об адресе сокета
		             hints;         // Хранит в себе настройки для создания сокета

#pragma region Pointer example
	//int n = 5;
	//int* n_ptr = &n;
	//printf("Type *n_ptr: %s\nValue *n_ptr: %d\n", typeid(*n_ptr).name(), *n_ptr);
#pragma endregion

	ZeroMemory(&hints, sizeof(hints)); // Инициализируем нулями

	// Настраиваем серверный сокет
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;
	hints.ai_flags = AI_PASSIVE; // Серверному сокету автоматически привяжется адрес локального узла

	funcResult = getaddrinfo(NULL, SERVER_PORT, &hints, &result);

	if (funcResult)
	{
		printf("Error #%d: getaddrinfo() failed!\n", funcResult);
		WSACleanup();

		return 1;
	}

	// Создаём серверный сокет прослушивания
	SOCKET serverSocket = INVALID_SOCKET;
	ptr = result; // На то, что раньше указывал result, теперь указывает ptr
	serverSocket = socket(ptr->ai_family, ptr->ai_socktype, ptr->ai_protocol); // -> оператор позволяет получить значение ссылки и достать содержимое его поля

	if (serverSocket == INVALID_SOCKET)
	{
		printf("Error #%d: socket() failed!\n", WSAGetLastError());
		freeaddrinfo(result);
		WSACleanup();

		return 1;
	}

	// Выводим имя и адрес сервера
	char host[256];
	char* ip;
	struct hostent* host_entry;
	int hostname = gethostname(host, sizeof(host));
	printf("\nServer name: %s\n", host);
	host_entry = gethostbyname(host);
	int iter = 0;
	while ((struct in_addr*)host_entry->h_addr_list[iter])
	{
		ip = inet_ntoa(*((struct in_addr*)host_entry->h_addr_list[iter++]));
		printf("Server address: %s:%s\n", ip, SERVER_PORT);
	}

	// Привязываем наш сервер к с своему сокету
	funcResult = bind(serverSocket, result->ai_addr, (int)result->ai_addrlen);

	if (funcResult == SOCKET_ERROR)
	{
		printf("Error #%d: bind() failed!\n", funcResult);
		freeaddrinfo(result);
		closesocket(serverSocket);
		WSACleanup();

		return 1;
	}

	freeaddrinfo(result); // Избавляемся от ненужной структуры

	// Прослушиваем сокет...
	if (listen(serverSocket, SOMAXCONN) == SOCKET_ERROR)
	{
		printf("Error #%d: listen() failed!\n", WSAGetLastError());
		closesocket(serverSocket);
		WSACleanup();

		return 1;
	}

	// Подтверждаем подключение
	printf("\nServer waiting for connection...\n\n");

	// Инициализируем таймер
	LARGE_INTEGER clockFrequency, startTime, finishTime, deltaTime;
	startTime.QuadPart = 0;
	QueryPerformanceFrequency(&clockFrequency);

	for (int i = 0; i < _requiredClientCount; i++)
	{
		SOCKET clientSocket = accept(serverSocket, NULL, NULL);

		if (clientSocket != INVALID_SOCKET)
		{
			DWORD threadId;
			CreateThread(NULL, NULL, clientHandler, &clientSocket, NULL, &threadId);

			if (i == _requiredClientCount - 1)
			{
				// Запускаем таймер
				QueryPerformanceCounter(&startTime);
			}
		}
		else
		{
			printf("Error: Unnable to accept client socket!\n");
		}
	}

	printf("Server waiting for clients answere...\n");
	while (_readyClientCount < _requiredClientCount);

	// Останавливаем таймер
	QueryPerformanceCounter(&finishTime);
	deltaTime.QuadPart = finishTime.QuadPart - startTime.QuadPart;
	float deltaMs = ((float)deltaTime.QuadPart) / clockFrequency.QuadPart * 1000.0f;
	printf("\nCommon square: %f\n", _commonSquare);
	printf("Timer finish: %f ms\n\n", deltaMs);

	// Отключаем сервер
	printf("Server finished to work...\n");
	closesocket(serverSocket);
	WSACleanup();
	system("pause");

	return 0;
}

DWORD WINAPI clientHandler(LPVOID clientSocket)
{
	int clientId = _maxOnlineClientCount;
	SOCKET socket = ((SOCKET*)clientSocket)[0];

	int distance = _finalInterval - _initialInterval;
	float subDistance = distance / (float)_requiredClientCount;

	Message mes = { clientId, _initialInterval + subDistance * clientId, _initialInterval + subDistance * (clientId + 1), _precision, 0 };
	void* mesPtr = &mes;

	printf("Client #%d conneceted to server...\n", clientId);
	printf("Server online: %d/%d clients...\n\n", ++_onlineClientCount, _requiredClientCount);
	++_maxOnlineClientCount;

	// Ждём всех клиентов
	while (_onlineClientCount < _requiredClientCount);

	// Отправляем сообщение клиенту
	int bytesCount;

	bytesCount = send(socket, (char*)&mes, sizeof(mes), 0);

	if (bytesCount > 0)
	{
		printf("Server sent message size of %d bytes\n", bytesCount);
		printf("Server sent message: { %d, %f, %f, %d, %f }\n\n", mes.client_id, mes.initial_interval, mes.final_interval, mes.precision, mes.square);
	}
	else
	{
		printf("Error: Unnabe to send message to client\n\n");
	}

	// Получаем ответ от клиента
	bytesCount = recv(socket, (char*)&mes, sizeof(mes), 0);

	if (bytesCount > 0)
	{
		printf("Server recieved message size of %d bytes\n", bytesCount);
		printf("Server recieved message: { %d, %f, %f, %d, %f }\n\n", mes.client_id, mes.initial_interval, mes.final_interval, mes.precision, mes.square);
		_commonSquare += mes.square;
	}
	else
	{
		printf("Error: Unnabe to recieved message from client\n\n");
	}

	printf("Client #%d disconnected from server...\n", clientId);
	printf("Server online: %d/%d clients, %d of them are ready...\n", --_onlineClientCount, _requiredClientCount, ++_readyClientCount);
	closesocket(socket);

	return 0;
}
#pragma endregion

#pragma region Worst example
//// Библиотеки
//#include <stdio.h> // Ввод-вывод
//
//#define _WINSOCK_DEPRECATED_NO_WARNINGS
//#pragma comment(lib, "Ws2_32.lib") // Запрашиваем обязательный доступ к библиотеке, для вызова WSAStartup
//#include <winsock2.h> // Для работы с сокетами
//
//// Макросы
//#define PORT 123 // Серверный порт
//#define PRINTNUSERS if (_nclients) printf("Users online: %d\n", _nclients); else printf("Users online: 0\n"); // Вывод онлайна
//
//// Прототипы
//DWORD WINAPI NewClient(LPVOID client_socket); // Обслуживаем подключившегося пользователя
//
//// Глобальные переменные
//int _nclients; // Количество онлайн пользователей
//
//int main(int argc, char* argv[])
//{
//    printf("Server started...\n");
//
//    // Создание переменных
//    char buff[1024]; // TODO: ???
//    WSADATA wsaData;
//
//    // Инициализация библиотеки
//    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0)
//    {
//        printf("Error WSAStartup %d\n", GetLastError());
//    }
//
//    // Создание сокета
//    SOCKET serverSocket = socket(AF_INET, SOCK_STREAM, 0);
//
//    if (serverSocket < 0)
//    {
//        printf("Error mySocket %d\n", GetLastError());
//        WSACleanup(); // Освобождаем память выделенную библиотеке
//
//        return 1;
//    }
//
//    // Связываем сокет с локальным адресом
//    sockaddr_in local_addr;
//    local_addr.sin_family = AF_INET;
//    local_addr.sin_port = htons(PORT);
//    local_addr.sin_addr.s_addr = 0; // так как сервер, принимаем подключения на все свои IP-адреса
//
//    if (bind(serverSocket, (sockaddr*)&local_addr, sizeof(local_addr)))
//    {
//        printf("Error bind %d\n", WSAGetLastError());
//        closesocket(serverSocket); // Освобождаем память затраченную на сокет
//        WSACleanup();
//
//        return 1;
//    }
//
//    // Ожидаем подключений с размером очереди 0x100 (256)
//    if (listen(serverSocket, 0x100))
//    {
//        printf("Error listen %d\n", WSAGetLastError());
//        closesocket(serverSocket);
//        WSACleanup();
//
//        return 1;
//    }
//
//    printf("Waiting for connections...\n");
//
//    // Извлекаем сообщение из очереди
//    SOCKET clientSocket;
//    sockaddr_in clientAddr; // адрес клиента (заполняется системой)
//    int clientAddrSize = sizeof(clientAddr);
//
//    // Цикл извлечения запросов на подключение из очереди
//    while (clientSocket = accept(serverSocket, (sockaddr*)&clientAddr, &clientAddrSize))
//    {
//        // Если удалось принять подключение
//        _nclients++;
//        //getnameInfo
//        HOSTENT* hst = gethostbyaddr((char*)&clientAddr.sin_addr.s_addr, 4, AF_INET); // Имя хоста
//
//        // Выводим сведения о клиенте
//        printf("+%s[%s:%d] new connect!\n", (hst) ? hst->h_name : "Unknown host", inet_ntoa(clientAddr.sin_addr), ntohs(clientAddr.sin_port));
//        PRINTNUSERS
//
//        // Вызов нового потока для обслуживания клиента
//        DWORD thID;
//        CreateThread(NULL, NULL, NewClient, &clientSocket, NULL, &thID);
//    }
//
//    // Освобождаем ресурсы
//    closesocket(serverSocket);
//    WSACleanup();
//
//    printf("Server finished...\n");
//    return 0;
//}
//
//// Обслуживание клиента в отдельном потоке
//DWORD WINAPI NewClient(LPVOID client_socket)
//{
//    SOCKET socket = ((SOCKET*)client_socket)[0];
//
//    if (socket == INVALID_SOCKET)
//    {
//        printf("NEWCLIENT ERROR\n");
//        return 1;
//    }
//    else
//    {
//        printf("NEWCLIENT EVERYTHING NICE :)\n");
//    }
//
//    return 0;
//}
#pragma endregion