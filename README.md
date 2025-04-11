# 🍽️ VideoDiningApp

A full-stack web application that blends **online food ordering** with the **joy of virtual dining**, allowing users to order food, dine with friends over video calls, and manage everything from cart to payment in a seamless experience.

## 🔗 Live Demo

> 🚧 **Demo Link:** _Coming Soon_

---

## 📖 Table of Contents

- [About the Project](#about-the-project)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [API Overview](#api-overview)
- [Getting Started](#getting-started)
- [Screenshots](#screenshots)
- [Future Enhancements](#future-enhancements)
- [Contact](#contact)

---

## 📌 About the Project

**VideoDiningApp** is an innovative social food ordering platform that enables users to:

- Browse and order food
- Add friends and dine virtually via live video calls
- Securely pay using OTP-based flows
- Manage orders and deliveries
- And for admins: control users, food, orders, and more

Built with modern tools and a clean architecture, this app bridges the gap between **food delivery** and **social connection**, perfect for remote hangouts, dates, or virtual parties.

---

## ✨ Features

### 👤 Authentication
- Secure Register/Login system
- JWT-based session handling
- OTP/Email verification (optional)

### 🛍️ Ordering System
- Browse available dishes
- Add/remove from cart
- Create and track orders
- Order history and delivery status

### 🧑‍💼 Admin Dashboard
- Dashboard stats (orders, revenue, users)
- Full CRUD on food items
- View/edit/delete users and orders
- Monitor all payments

### 💳 Secure Payments
- OTP-based payment confirmation
- Generate and verify OTPs
- Mark payments as successful

### 👥 Friends & Social Dining
- Add, accept, reject, and remove friends
- View friend list and pending requests
- Check friendship status

### 🎥 Video Dining
- Start and end video calls per order
- View call participants
- View call history by user
- (WebRTC integration suggested)

---

## 🛠️ Tech Stack

| Layer          | Tech Used                         |
|----------------|-----------------------------------|
| Frontend       | React.js, Tailwind CSS, Axios     |
| Backend        | ASP.NET Core Web API              |
| Database       | SQL Server                        |
| Auth           | JWT, ASP.NET Identity (optional)  |
| Video Calls    | WebRTC / SignalR (planned)        |
| Tools          | Swagger, Postman, Git, VS Code    |

---


---

## 📡 API Overview

The API is organized into modules and served with Swagger:

> **Swagger UI:** `http://localhost:5289/swagger/index.html`

### ✅ Admin Endpoints

- `GET /api/admin/dashboard`
- `POST /api/admin/login`
- Manage users: `GET`, `DELETE /api/admin/users`
- Manage foods: `GET`, `POST`, `PUT`, `DELETE /api/admin/foods`
- View and update orders
- View payments

### 🔐 Auth Endpoints

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/verify`

### 🛒 Cart Endpoints

- `GET /api/cart/{userId}`
- `POST /api/cart/add`
- `DELETE /api/cart/remove/{cartItemId}`

### 🍴 Food

- `GET /api/foods`

### 🧑‍🤝‍🧑 Friends

- Add, Accept, Reject: `POST /api/friends/add|accept|reject`
- `DELETE /api/friends/remove`
- `GET /api/friends/list/{userId}`
- `GET /api/friends/check/{userId}/{friendId}`
- `GET /api/friends/requests/{userId}`

### 📦 Orders

- `POST /api/orders/create/{userId}`
- `GET /api/orders`, `/history/{userId}`, `/status/{orderId}`
- `POST /api/orders/update-status/{orderId}`
- `POST /api/orders/mark-delivered/{orderId}/{userId}`
- `POST /api/orders/remind/{orderId}`
- `DELETE /api/orders/cancel/{orderId}/{userId}`

### 💸 Payment (OTP-secured)

- `POST /api/payment/send-links`
- `POST /api/payment/verifyOtp`
- `POST /api/payment/generateOtp`
- `POST /api/payment/confirm-payment`
- `POST /api/payment/pay/{orderId}`
- `POST /api/payment/success`

### 📹 Video Call

- `POST /api/video-call/start`
- `POST /api/video-call/end`
- `GET /api/video-call/{orderId}/participants`
- `GET /api/video-call/{orderId}`
- `GET /api/video-call/history/{userId}`

---

## 🚀 Getting Started

### Prerequisites

- Node.js & npm
- .NET 6 SDK or later
- SQL Server (LocalDB or SQLExpress)
- Visual Studio / VS Code

### 🔧 Setup Instructions

1. **Clone the repo**

git clone https://github.com/yourusername/VideoDiningApp.git
cd VideoDiningApp

2. **Install Frontend Dependencies**

git clone https://github.com/yourusername/VideoDiningApp.git
cd VideoDiningApp

3. **Setup Backend**

cd ../server
dotnet restore
dotnet ef database update
dotnet run

4. **Open in Browser**

Frontend: http://localhost:3000
Backend API (Swagger): http://localhost:5289/swagger

🖼️ Screenshots

🔮 Future Enhancements
🔴 Real-time chat during video calls

📱 Mobile responsive layout

🌐 OAuth (Google, Facebook)

📍 Live order tracking on maps

🌟 Dish ratings & reviews

📢 Push notifications

🤝 Contact
Created by: [Your Name]

📧 Email: siranjeeviwd@gmail.com
💼 LinkedIn: [linkedin.com/in/siranjeevis01](https://www.linkedin.com/in/siranjeevis01/)
💻 GitHub: [github.com/siranjeevis01](https://github.com/siranjeevis01)


---

If you'd like, I can also help:
- Add **badges** (build, license, etc.)
- Create a `Postman` collection for API testing
- Generate database diagrams or ERD
- Write deployment instructions (for Vercel, Azure, etc.)

Just let me know!
