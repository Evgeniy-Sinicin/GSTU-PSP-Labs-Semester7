#include <winsock2.h>
#include <ws2tcpip.h>
#include <stdio.h>
#include <typeinfo>
#include <iphlpapi.h>
#include <iostream>
#include <fstream>
#include <iomanip>
#include <string>
#include <vector>
#include "matrix.h"

#pragma comment(lib, "Ws2_32.lib")
#pragma comment(lib, "iphlpapi.lib")

#define SERVER_PORT "27015"

using namespace std;

DWORD WINAPI handle_client(LPVOID context);
template <typename T> Matrix<T> read_matrix(string path);
template <typename T> void print_matrix(Matrix<T>& matrix, int rows_count, int columns_count, string label, int precision);
template <typename T> void matrix_to_array(Matrix<T>& matrix, T* array);
template <typename T> Matrix<T> array_to_matrix(T* array, int rows_count, int columns_count);

const int SYSTEM_SIZE = 3;
const int COEFFS_LENGTH = 1;

struct Message
{
	int client_index;
	float system_arr[SYSTEM_SIZE * SYSTEM_SIZE];
	float coeffs_arr[SYSTEM_SIZE];
	float decision_arr[SYSTEM_SIZE];
};

extern vector<Message> _messages = vector<Message>();
extern vector<SOCKET> _client_sockets = vector<SOCKET>();

int _client_index;
int _requiredClientCount;
INT64 _counterStart;
LARGE_INTEGER _public_time;

int main(int argc, char* argv[])
{
	printf("Server started to work...\n\n");
	printf("Enter required client count: ");
	cin >> _requiredClientCount;

	int funcResult;
	vector<HANDLE> handles;
	for (int i = 0; i < _requiredClientCount; i++)
	{
		string path = "./Lab_2_Resources/";
		string system_path = path + "Matrix_" + to_string(i) + ".txt";
		string coeffs_path = path + "Coeffs_" + to_string(i) + ".txt";
		Matrix<float> system = read_matrix<float>(system_path);
		Matrix<float> coeffs = read_matrix<float>(coeffs_path);
		Matrix<float> decision(coeffs.getRows(), coeffs.getColumns());
		Message message;
		message.client_index = i;
		matrix_to_array(system, message.system_arr);
		matrix_to_array(coeffs, message.coeffs_arr);
		_messages.push_back(message);
	}
	WSADATA wsaData;

	funcResult = WSAStartup(MAKEWORD(2, 2), &wsaData);

	if (funcResult)
	{
		printf("Error #%d: WSAStartup failed!\n", funcResult);
	}

	struct addrinfo* result = NULL, // Требуется для получения информации об адресе сокета
				   * ptr = NULL,    // Требуется для хранения информации об адресе сокета
		             hints;         // Хранит в себе настройки для создания сокета

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
	ptr = result;															   // На то, что раньше указывал result, теперь указывает ptr
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
	LARGE_INTEGER clockFrequency, startTime, finishTime, deltaTime; // Инициализируем таймер
	//startTime.QuadPart = 0;
	_public_time.QuadPart = 0;
	QueryPerformanceFrequency(&clockFrequency);

	for (int i = 0; i < _requiredClientCount; i++)
	{
		SOCKET _client_socket = accept(serverSocket, NULL, NULL);
		_client_sockets.push_back(_client_socket);

		if (_client_socket != INVALID_SOCKET)
		{
			// Требуется для того, чтобы сообщить главному потоку о завершении работы или другом событии
			// Параметры:
			// 1. Атрибут безопасности
			// 2. Событие ручного сброса?
			// 3. Начальное состояние (true - вкл/false - выкл)
			// 4. Название событийного объекта
			HANDLE handle = CreateEvent(NULL, TRUE, FALSE, to_wstring(i).c_str());
			if (handle == NULL)
			{
				perror("CreateEvent");
				return EXIT_FAILURE;
			}
			handles.push_back(handle);

			// Помещаем функцию в рабочий поток из пула потоков
			// Параметры:
			// 1. Помещаемая функция
			// 2. Параметр помещаемой функции, требуется для TDOO: Узнать для)
			// 3. Один из флагов управления, которые бывают:
			//      A. WT_EXECUTEDEFAULT - поток не для ввода-вывода
			//      B. WT_EXECUTEINIOTHREAD - поток ожидающий ввод-вывод
			//      C. WT_EXECUTEINPERSISTENTTHREAD - пожизненный поток
			//      D. WT_EXECUTELONGFUNCTION - позволяет создать новый поток, если текущий займёт чересчур много времени
			//      E. WT_TRANSFER_IMPERSONATION - требуется для того, чтобы callback функция использовала текущий токен доступа
			QueueUserWorkItem(handle_client, (PVOID)handle, WT_EXECUTEDEFAULT);

			//DWORD threadId;
			//CreateThread(NULL, NULL, handle_client, &clientSocket, NULL, &threadId);

			//if (i == _requiredClientCount - 1)
			//{
			//	// Запускаем таймер
			//	QueryPerformanceCounter(&startTime);
			//}
		}
		else
		{
			printf("Error: Unnable to accept client socket!\n");
		}
	}

	printf("Server waiting for clients answere...\n");
	for (int i = 0; i < handles.size(); i++)
	{
		WaitForSingleObject(handles[i], INFINITE);
		CloseHandle(handles[i]);
	}

	// Останавливаем таймер
	//QueryPerformanceCounter(&finishTime);
	//deltaTime.QuadPart = finishTime.QuadPart - startTime.QuadPart;
	//float deltaMs = ((float)deltaTime.QuadPart) / clockFrequency.QuadPart * 1000.0f;
	printf("Timer finish: %d ns\n\n", _public_time.QuadPart);

	// Отключаем сервер
	printf("Server finished to work...\n");
	closesocket(serverSocket);
	WSACleanup();
	system("pause");

	return EXIT_SUCCESS;
}

DWORD WINAPI handle_client(LPVOID context)
{
	LARGE_INTEGER start_time, finish_time, delta_time, clockFrequency;
	QueryPerformanceFrequency(&clockFrequency);
	QueryPerformanceCounter(&start_time);

	int precision = 5;
	HANDLE handle_event = (HANDLE)context;

	Message message = _messages[_client_index];
	SOCKET client_socket = _client_sockets[_client_index];
	message.client_index = _client_index++;

	string name = "System #";
	string number = to_string(message.client_index);
	string name_and_number = name + number;
	Matrix<float> system = array_to_matrix(message.system_arr, SYSTEM_SIZE, SYSTEM_SIZE);
	Matrix<float> coeffs = array_to_matrix(message.coeffs_arr, SYSTEM_SIZE, COEFFS_LENGTH);
	//print_matrix(system, system.getRows(), system.getColumns(), name_and_number, precision);
	name = "Coeffs #";
	name_and_number = name + number;
	//print_matrix(coeffs, coeffs.getRows(), coeffs.getColumns(), name_and_number, precision);
	name = "Decision #";
	name_and_number = name + number;

	//
	printf("Client %d/%d conneceted to server...\n", message.client_index + 1, _requiredClientCount);

	// Ждём всех клиентов
	//while (message.client_index < _requiredClientCount - 1);

	// Отправляем сообщение клиенту
	int bytesCount = send(client_socket, (char*)&message, sizeof(message), 0);
	if (bytesCount > 0)
	{
		printf("Server sent client's:%d message size of %d bytes\n", message.client_index, bytesCount);
	}
	else
	{
		printf("Error: Unnabe to send message #%d to client\n\n", message.client_index);
	}

	// Получаем ответ от клиента
	bytesCount = recv(client_socket, (char*)&message, sizeof(message), 0);
	Matrix<float> decision = array_to_matrix(message.decision_arr, SYSTEM_SIZE, COEFFS_LENGTH);

	if (bytesCount > 0)
	{
		printf("Server recieved message size of %d bytes\n", bytesCount);
		print_matrix(decision, decision.getRows(), decision.getColumns(), name_and_number, precision);
	}
	else
	{
		printf("Error: Unnabe to receive message #%d from client\n\n", message.client_index);
	}

	QueryPerformanceCounter(&finish_time);
	delta_time.QuadPart = finish_time.QuadPart - start_time.QuadPart;
	_public_time.QuadPart += delta_time.QuadPart;
	printf("Client #%d disconnected from server (after %d ns.)...\n\n", message.client_index, delta_time.QuadPart);
	closesocket(client_socket);
	//

	SetEvent(handle_event);

	return EXIT_SUCCESS;
}

template <typename T> Matrix<T> read_matrix(string path)
{
	int rows_count, columns_count;
	string file_line;
	string delimeter = " ";
	ifstream file(path);

	if (!file.is_open())
	{
		throw exception("Unnable to read file!");
	}

	getline(file, file_line);
	rows_count = stoi(file_line);
	getline(file, file_line);
	columns_count = stoi(file_line);
	Matrix<T> matrix(rows_count, columns_count);

	for (int i = 0; i < rows_count; i++)
	{
		getline(file, file_line);

		for (int j = 0; j < columns_count; j++)
		{
			int delimeter_position = file_line.find(delimeter);
			matrix.put(i, j, stof(file_line.substr(0, delimeter_position)));
			file_line.erase(0, delimeter_position + delimeter.length());
		}
	}

	file.close();

	return matrix;
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