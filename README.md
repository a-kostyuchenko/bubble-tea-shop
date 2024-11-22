# Bubble Tea Shop

Bubble Tea Shop is a microservices-based application that allows managing a bubble tea shop, including orders, payments, invoices and more. The application is built using the latest versions of .NET and .NET Aspire.

## Table of Contents

- [Features](#features)
- [Technologies Used](#technologies-used)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

## Features

- **Bubble Tea Menu:** Display a list of bubble tea products with prices.
- **Order Management:** Create, track order status, cancel and complete orders.
- **Adjustable Ingredients and Parameters:** Customize bubble tea ingredients and quantities as you wish, set specific parameters for each product.
- **Parameterized Discounts:** Apply discounts based on order total, quantity, or specific products. (TODO)
- **Invoice Generation:** Generate invoices for orders and payments.
- **Reporting:** Generate reports on sales, orders, and other metrics. (TODO)
- **Localization:** Support multiple languages and currencies. (TODO)
- **Notifications:** Send notifications for order status updates, promotions, etc. (TODO)

## Technologies Used

- **C#**
- **.NET 9**
- **.NET Aspire (latest version)**
- **XUnit:** Testing framework for unit tests.
- **Database:** PostgreSQL for data storage.
- **ORM:** Entity Framework Core for object-relational mapping.
- **Messaging:** RabbitMQ for messaging between services.
- **Caching:** Redis for caching.
- **CI/CD:** GitHub Actions for continuous integration and deployment.

## Architecture

Bubble Tea Shop follows a microservices architecture, which includes:

- **API Gateway:** Exposes endpoints for communication with the application.
- **Services:** Contains the business logic, data access, and presentation for each service.
- **Tests:** Includes test projects for the application.

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download)
- [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire)

## Project Structure

- `src/`
    - `services/` - Contains the microservices for the application.
        - `ordering-api/` - Service for managing orders.
        - `payment-api/` - Service for processing payments.
        - `catalog-api/` - Service for managing the bubble tea menu.
        - `cart-api/` - Service for managing shopping carts.
    - `gateway/` - API Gateway for routing requests to the appropriate service.
    - `contracts/` - Shared contracts between services.
    - `aspire/` - Contains the .NET Aspire projects.

## Contributing

Contributions are welcome! Please fork the repository and create a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact

For any inquiries, please contact [kosttchka@gmail.com](mailto:kosttchka@gmail.com).

Feel free to adjust the content to fit your specific project details and any additional information you would like to include.
