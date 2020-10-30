#include <iostream>
#include <fstream>
#include <iomanip>
#include <string>
#include <winsock2.h> 
#include <ws2tcpip.h> 
#include <windows.h>
#include <stdio.h> 
#include <cmath>
#include "matrix.h"

#pragma comment(lib, "Ws2_32.lib")

#define DEFAULT_BUFFER_LENGTH 1024

using namespace std;

const int SYSTEM_SIZE = 3;
const int COEFFS_LENGTH = 1;

struct Message
{
	int client_index;
	float system_arr[SYSTEM_SIZE * SYSTEM_SIZE];
	float coeffs_arr[SYSTEM_SIZE];
	float decision_arr[SYSTEM_SIZE];
};

template <typename T> void print_matrix(Matrix<T>& matrix, int rows_count, int columns_count, string label, int precision);
template <typename T> void matrix_to_array(Matrix<T>& matrix, T* array);
template <typename T> Matrix<T> array_to_matrix(T* array, int rows_count, int columns_count);

int main()
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

	struct addrinfo* result,
				   * ptr,
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
	Message message;
	int bytesCount = recv(serverSocket, (char*)&message, sizeof(message), 0);
	if (bytesCount <= 0)
	{
		printf("Error: Unnable to receive server message\n\n");
		return EXIT_FAILURE;
	}

	int precision = 5;
	printf("Client #%d got message size of %d bytes...\n", message.client_index, bytesCount);
	Matrix<float> system_matrix = array_to_matrix(message.system_arr, SYSTEM_SIZE, SYSTEM_SIZE);
	Matrix<float> coeffs = array_to_matrix(message.coeffs_arr, SYSTEM_SIZE, COEFFS_LENGTH);
	Matrix<float> inverse = system_matrix.getInverse();
	Matrix<float> decision = inverse * coeffs;

	print_matrix<float>(system_matrix, SYSTEM_SIZE, SYSTEM_SIZE, "Init Matrix", precision);
	print_matrix<float>(coeffs, coeffs.getRows(), coeffs.getColumns(), "Init Coeffs", precision);
	print_matrix<float>(inverse, SYSTEM_SIZE, SYSTEM_SIZE, "Inverse Matrix", precision * 2);
	print_matrix<float>(decision, coeffs.getRows(), coeffs.getColumns(), "Decision Matrix", precision);

	matrix_to_array(decision, message.decision_arr);
	bytesCount = send(serverSocket, (char*)&message, sizeof(Message), 0);
	if (bytesCount <= 0)
	{
		printf("Error: Unnable to send client message\n\n");
		return EXIT_FAILURE;
	}
	printf("Client #%d sent message size of %d bytes...\n", message.client_index, bytesCount);

	printf("Client #%d completed work with server...\n", message.client_index);
	funcResult = shutdown(serverSocket, SD_RECEIVE);

	if (funcResult == SOCKET_ERROR)
	{
		printf("Error #%d: shutdown() failed!\n", funcResult);
		closesocket(serverSocket);
		WSACleanup();

		return EXIT_FAILURE;
	}

	printf("Client finished to work...\n");

	// Освобождаем ресурсы
	closesocket(serverSocket);
	WSACleanup();
	system("pause");

    return EXIT_SUCCESS;
}

template <typename T> void print_matrix(Matrix<T>& matrix, int rows_count, int columns_count, string label, int precision)
{
    cout << "--- " << label << " ---" << endl;

    for (int i = 0; i < rows_count; i++)
    {
        for (int j = 0; j < columns_count; j++)
        {
            cout << setw(precision) << matrix.get(i, j);
        }

        cout << endl;
    }

    cout << "--- --- --- --- ---" << endl;
}

template <typename T> void matrix_to_array(Matrix<T>& matrix, T* array)
{
	for (int i = 0; i < matrix.getRows(); i++)
	{
		for (int j = 0; j < matrix.getColumns(); j++)
		{
			array[i * matrix.getColumns() + j] = matrix.get(i, j);
		}
	}
}

template <typename T> Matrix<T> array_to_matrix(T* array, int rows_count, int columns_count)
{
	Matrix<T> matrix(rows_count, columns_count);

	for (int i = 0; i < rows_count; i++)
	{
		for (int j = 0; j < columns_count; j++)
		{
			matrix.put(i, j, array[i * columns_count + j]);
		}
	}

	return matrix;
}