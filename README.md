# Pantsu Tap Playground - Replenishment API

## Overview

The **Replenishment API** is a core component of the Pantsu Tap project, designed to handle replenishment transactions within the system. This API allows users to create and execute transactions securely, interacting with the Solana blockchain.

## Project Details

- **Developed by**: Olexandr Harmash
- **Date**: [date]

## API Endpoints

### Create Transaction

- **Endpoint**: `POST /api/replenishment/transaction`
- **Description**: Creates a new replenishment transaction. The client sends transaction details including public keys and amount, and the server returns transaction data to be signed.
- **Request Body**: `TransactionRequestDto`
- **Responses**: 
  - `200 OK`: Returns transaction details to be signed.
  - `400 Bad Request`: If required headers or body data are missing.

### Execute Signed Transaction

- **Endpoint**: `PUT /api/replenishment/transaction`
- **Description**: Executes a signed replenishment transaction. The client sends the signed transaction, and the server processes it.
- **Request Body**: `TransactionSignedDto`
- **Responses**: 
  - `200 OK`: Returns the result of the transaction execution.
  - `400 Bad Request`: If the signed transaction is invalid or missing.

## TODOs

- **Implement JWT Authorization**: Secure endpoints with JWT to restrict access.
- **Store Transaction Data**: Implement logic to store and validate transaction data.

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

3. Access the API at `http://localhost:8080`.

### Configuration

- **Environment Variables**:
  - `SERVER_PASS_PHRASE`: The passphrase used for wallet operations.
  - `SERVER_KEY_PAIR_FILE`: The path to the keypair file used for wallet initialization.

## License

This project is licensed under the [MIT License](LICENSE).
