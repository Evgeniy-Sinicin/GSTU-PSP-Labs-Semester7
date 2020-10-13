#pragma region Worst example
//#include <stdio.h>
//#include <string.h>
//#include <winsock2.h>
//#include <windows.h>
//#include <Ws2tcpip.h>
//
//#pragma comment(lib, "Ws2_32.lib") // Запрашиваем обязательный доступ к библиотеке, для вызова WSAStartup
//#pragma warning(disable:4996) // Запрашиваем обязательный доступ к библиотеке, для вызова WSAStartup
//
//#define SERVER_PORT 27015
//#define SERVER_ADDRES "127.0.0.1"
//
//int main(int argc, char* argv[])
//{
//	printf("Client started...\n");
//
//	char buff[1024]; // Буфер для передачи сообщений
//
//	if (WSAStartup(0x202, (WSADATA*)&buff[0]))
//	{
//		printf("Error #%d: WSAStartup() failed!\n", WSAGetLastError());
//
//		return 1;
//	}
//
//	// Создаём сокет
//	SOCKET clientSocket = socket(AF_INET, SOCK_STREAM, 0);
//
//	if (clientSocket == INVALID_SOCKET)
//	{
//		printf("Error #%d: socket() failed!\n", WSAGetLastError());
//
//		return 1;
//	}
//
//	// Устанавливаем соединение
//	HOSTENT* hst;
//	sockaddr_in dest_addr;
//	dest_addr.sin_family = AF_INET;
//	dest_addr.sin_port = htons(SERVER_PORT);
//
//	// Преобразование IP адреса из символьного в сетевой формат
//	if (inet_addr(SERVER_ADDRES) != INADDR_NONE)
//	{
//		dest_addr.sin_addr.s_addr = inet_addr(SERVER_ADDRES);
//		printf("YES: inet_addr = %s\n", inet_ntoa(dest_addr.sin_addr));
//	}
//	else if (hst = gethostbyname(SERVER_ADDRES))
//	{
//		((unsigned long*)&dest_addr.sin_addr)[0] = ((unsigned long**)hst->h_addr_list)[0][0];
//		printf("YES IF: inet_addr = %s\n", inet_ntoa(dest_addr.sin_addr));
//	}
//	else
//	{
//		printf("Error #%d: Invalid addres %s!\n", WSAGetLastError(), SERVER_ADDRES);
//		closesocket(clientSocket);
//		WSACleanup();
//
//		return 1;
//	}
//
//	// Отправляем и читаем сообщения	
//	int size;
//
//	while ((size = recv(clientSocket, &buff[0], sizeof(buff)[1], 0)) != SOCKET_ERROR)
//	{
//		buff[size] = 0; // Завершающий ноль
//		
//		printf("S => C: %s\n", buff);
//		printf("S <= C: ");
//		fgets(&buff[0], sizeof(buff)[1], stdin);
//
//		if (!strcmp(&buff[0], "quit\n"))
//		{
//			printf("Client done...\n");
//			closesocket(clientSocket);
//			WSACleanup();
//
//			return 0;
//		}
//
//		send(clientSocket, &buff[0], size, 0);
//	}
//
//	printf("Error #%d: recv() failed!\n", WSAGetLastError());
//
//	// Освобождаем память
//	closesocket(clientSocket);
//	WSACleanup();
//
//	printf("Client finished...\n");
//
//	return 0;
//}
#pragma endregion

#include <winsock2.h> 
#include <ws2tcpip.h> 
#include <stdio.h> 
#include <cmath>

#pragma comment(lib, "Ws2_32.lib")

#define DEFAULT_BUFFER_LENGTH 512

struct Message
{
	const int client_id;
	const float initial_interval;
	const float final_interval;
	const int precision;
	float square;
};

void Square(Message* mes);
float Square(float x1, float x2);
float Func(float x);

int main(int argc, char* argv[])
{
	printf("Client started to work...\n\n");
	printf("Eneter server addres: ");
	char serverAddress[INET_ADDRSTRLEN];// = "192.168.43.21" "192.168.42.38";
	scanf("%s", serverAddress);
	//printf("Eneter server port: ");
	char serverPort[6] = "27015";
	//scanf("%s", serverPort);

	int funcResult;
	WSADATA wsaData;

	funcResult = WSAStartup(MAKEWORD(2, 2), &wsaData);

	if (funcResult)
	{
		printf("Error #%d: WSAStartup() failed!\n", funcResult);

		return 1;
	}

	struct addrinfo *result,
					*ptr,
					 hints;

	ZeroMemory(&hints, sizeof(hints));

	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;

	funcResult = getaddrinfo(serverAddress, serverPort, &hints, &result);

	if (funcResult)
	{
		printf("Error #%d: getaddrinfo() failed!\n", funcResult);
		WSACleanup();

		return 1;
	}

	printf("\nClient initialized...\n\n");
	ptr = result;
	SOCKET serverSocket = socket(ptr->ai_family, ptr->ai_socktype, ptr->ai_protocol);

	if (serverSocket == INVALID_SOCKET)
	{
		printf("Error #%d: socket() failed!\n", WSAGetLastError());
		freeaddrinfo(result);
		WSACleanup();

		return 1;
	}

	printf("Client created socket and tryies to connect...\n\n");
	funcResult = connect(serverSocket, ptr->ai_addr, (int)ptr->ai_addrlen);

	if (funcResult == SOCKET_ERROR)
	{
		printf("Error #%d: connect() failed!\n", funcResult);
		serverSocket = INVALID_SOCKET;

		return 1;
	}

	freeaddrinfo(result);

	if (serverSocket == SOCKET_ERROR)
	{
		printf("Unable connect to server!\n");
		WSACleanup();

		return 1;
	}

	printf("Client connected to server and waiting for messages...\n\n");

	// Получаем сообщение
	int bytesCount;
	char messageBuffer[sizeof(Message)];

	bytesCount = recv(serverSocket, messageBuffer, sizeof(Message), 0);

	Message* mes = (Message*)&messageBuffer;

	if (bytesCount > 0)
	{
		printf("Client got message size of %d bytes...\n", bytesCount);
		printf("Gotten message: { %d, %f, %f, %d, %f }\n\n", mes->client_id, mes->initial_interval, mes->final_interval, mes->precision, mes->square);
	}
	else
	{
		printf("Error: Unnable to receive server message\n\n");
	}

	Square(mes);
	printf("Client #%d calculated square: %f\n\n", mes->client_id, mes->square);

	bytesCount = send(serverSocket, messageBuffer, sizeof(Message), 0);

	if (bytesCount > 0)
	{
		printf("Client sent message size of %d bytes...\n", bytesCount);
		printf("Sent message: { %d, %f, %f, %d, %f }\n\n", mes->client_id, mes->initial_interval, mes->final_interval, mes->precision, mes->square);
	}
	else
	{
		printf("Error: Unnable to send client message\n\n");
	}

	printf("Client completed work with server...\n");
	funcResult = shutdown(serverSocket, SD_RECEIVE);

	if (funcResult == SOCKET_ERROR)
	{
		printf("Error #%d: shutdown() failed!\n", funcResult);
		closesocket(serverSocket);
		WSACleanup();

		return 1;
	}

	printf("Client finished to work...\n");

	// Освобождаем ресурсы
	closesocket(serverSocket);
	WSACleanup();
	system("pause");

	return 0;
}

void Square(Message* mes)
{
	float length = mes->final_interval - mes->initial_interval;
	float dx = length / mes->precision;

	for (int i = 0; i < mes->precision; i++)
	{
		float x1 = mes->initial_interval + (dx * i);
		float x2 = x1 + dx;

		mes->square += Square(x1, x2);
	}
}

float Square(float x1, float x2)
{
	return ((Func(x1) + Func(x2)) / 2.0f) * (x2 - x1);
}

float Func(float x)
{
	return fabs(expf(-x) * sinf(x));
}