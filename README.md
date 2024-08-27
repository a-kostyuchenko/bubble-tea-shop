# Bubble Tea Shop

This repository contains the source code for a bubble tea shop management system built using microservices architecture in .NET. It includes various services for handling orders, inventory, user management, and payments, leveraging PostgreSQL for database management.

## Features

- **Microservices Architecture:** Each service is independent, enabling easy maintenance and scaling.
- **Tech Stack:** 
  - .NET Core
  - PostgreSQL
  - .NET Aspire
- **RESTful API:** Each service exposes a RESTful API for interaction.

## Getting Started

### Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/a-kostyuchenko/bubble-tea-shop.git
   ```
2. Navigate to the project directory and set up the environment:
   ```bash
   cd bubble-tea-shop
   dotnet restore
   ```

3. Run the services:
   ```bash
   dotnet run
   ```

### Usage

After starting the services, you can access the APIs at `http://localhost:5000`. Documentation for each API endpoint can be found in the [API Documentation](docs/api.md).

### Testing

To run the tests, use the following command:
```bash
dotnet test
```

## Contributing

Contributions are welcome! Please read our [contributing guidelines](CONTRIBUTING.md) first.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact

For any inquiries, feel free to open an issue or contact the maintainer at kosttchka@gmail.com.

---
