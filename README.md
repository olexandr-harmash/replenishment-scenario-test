# Pantsu Tap Playground - Replenishment API

## Описание проекта

Проект предназначен для обработки транзакций пополнения в блокчейне Solana. С помощью данного API пользователи могут отправлять подписанные транзакции, которые затем проверяются и обрабатываются сервером. API обеспечивает версионирование и структурированное определение конечных точек для выполнения операций с транзакциями.

### Основные компоненты

#### Процесс выполнения транзакции

- **Получение и проверка авторизации:**

  - Клиент отправляет запрос с заголовком авторизации, содержащим токен. Если токен отсутствует или недействителен, сервер возвращает ошибку.

- **Обработка транзакции:**

  - Сервер получает подписанную транзакцию и извлекает из нее инструкции.
  - Если инструкция `Transfer` найдена, сервер фиксирует детали перевода и сохраняет их.

- **Отправка транзакции:**

  - Транзакция отправляется на выполнение через RPC-клиент.
  - Сервер обрабатывает ответ, логируя успех или неудачу.

- **Подписка на события:**

  - Сервер создает команду для подписки на события подтверждения транзакции и отправляет ее в медиатор.

### Реализовано в `Replenishment.Api.Apis.ExecuteTransaction`

- Принимает заголовок авторизации и тело запроса, которое содержит подписанную транзакцию в формате Base64.

- Извлекает токен (публичный ключ кошелька пользователя в данном примере) из заголовка авторизации и проверяет его на наличие.

- Десериализует транзакцию и компилирует сообщение.

- Декодирует инструкции транзакции, ищет инструкцию `Transfer`, которая отвечает за перевод средств.
 ```c#
  // Search for the Transfer instruction
    Transfer transferInstruction;
    try
    {
        var data = instructions.Single(i => i.InstructionName == nameof(Transfer));

        transferInstruction = new Transfer
        {
            From = (PublicKey)data.Values["From Account"],
            To = (PublicKey)data.Values["To Account"],
            Amount = (ulong)data.Values["Amount"]
        };

        // Log transfer details before sending
        services.Logger.LogInformation(
            "Transfer sending:\n From: {From}\n To: {To}\n Amount: {Amount}",
            transferInstruction.From,
            transferInstruction.To,
            transferInstruction.Amount);

        // Save the transfer to cache
        services.Cache.PullTransfer(token, transferInstruction);
    }
    catch (InvalidOperationException)
    {
        // Log an error if the instruction was not found or multiple were found
        services.Logger.LogError("More than one or no Transfer instructions were found.");
        return TypedResults.BadRequest("Transfer instruction not found or ambiguous.");
    }
 ```
- Отправляет транзакцию на выполнение через RPC-клиент.
 ```c#
  // Send the transaction and retrieve the signature
  var result = await services.RpcCilent.SendTransactionAsync(request.Base64TransactionData);

  if (!result.WasSuccessful)
  {
      // Log an error if the transaction failed to send
      services.Logger.LogError("Error sending the transaction.");
      return TypedResults.BadRequest("Error sending the transaction.");
  }
 ```
- Генерирует команду для подписки на события подтверждения транзакции и отправляет ее в медиатор для дальнейшей обработки.
 ```c#
  var requestTransfer = new SubscribeTransferCommand(result.Result, token);

  services.Logger.LogInformation(
      "Sending command: {CommandName}: ({@Command})",
      nameof(requestTransfer),
      requestTransfer);

  await services.Mediator.Publish(requestTransfer);
 ```


## Project Details

- **Developed by**: Olexandr Harmash
- **Date**: [date]

## API Endpoints

### Execute Transaction

- **Request Body**: `ExecuteTransaction`
  - Base64TransactionData: string
- **Responses**: 
  - `200 OK`: Returns nothing.
  - `400 Bad Request`: If required headers or body data are missing or transaction cant be validated and executed.



## TODOs

- **Implement JWT Authorization**: Secure endpoints with JWT to restrict access.
- **Store Transaction Data in storage**: Implement logic to store and validate transaction data.
- **Add pubkey of server from enfiroment in transfer model**: Add pubkey of server from enfiroment in transfer model.
- **Git ignore**: Add git ignore file.
- **Refactor react app**: Make proper useEffect checkers.
- **Refactor react app**: Add env variables.
- **Refactor react dockerfile**: Optimize.

## Documentation

- **Solnet GitHub Library**: [Solnet GitHub](https://github.com/solana-labs/solnet)
- **Confluence Page**: [Replenishment Service Details](https://olexandrharmash.atlassian.net/wiki/spaces/PT2/pages/12255233/Replenishment+Service)
- **Jira Task**: [PT-1](https://olexandrharmash.atlassian.net/browse/PT-1)

## Development

### Building and Running

1. Clone the repository:
    ```bash
    git clone https://github.com/your-repo-url.git
    cd your-repo-folder
    ```

2. Build and run the application using Docker:
    ```bash
    docker-compose up --build
    ```

3. Access the API at `http://localhost:3000`.

### Configuration

- **Environment Variables**:
  - `SERVER_PASS_PHRASE`: The passphrase used for wallet operations.
  - `SERVER_KEY_PAIR_FILE`: The path to the keypair file used for wallet initialization.

## License

This project is licensed under the [MIT License](LICENSE).
