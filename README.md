# 🍽️ Video Dining App – Enhancing Online Food Ordering with Real-Time Social Experience

Welcome to the **Video Dining App**, a groundbreaking full-stack web application that redefines online food ordering by integrating **real-time collaboration**, **group ordering**, and **virtual dining via video calls**.

---

## 🚀 Project Highlights

- **Real-Time Food Ordering** – Track orders as they're placed, prepared, and delivered.
- **Virtual Dining Experience** – Auto-triggered video call once food is delivered, using Jitsi Meet API.
- **Shared Cart** – Friends & family can add food collaboratively to a single order.
- **Split Payments** – Each participant pays their share securely with OTP verification.
- **Live Notifications** – Real-time updates via SignalR for order status and user activity.

---

## 🧠 Problem Statement

> Traditional food ordering platforms lack a collaborative experience, especially for remote friends/family who want to eat together. There's no shared cart, no real-time interaction, and no virtual dining feel.

### ✅ Our Solution:
We created a system that combines the **convenience of online ordering** with the **warmth of social dining**, no matter where users are located.

---

## 🧩 System Modules

- 👥 **User Management** – Login, registration, and friend system with request/accept/reject features.
- 🛒 **Ordering** – Menu browsing, collaborative cart, and order submission.
- 💸 **Payments** – Individual secure payments with transaction validation.
- 📦 **Tracking** – Real-time order updates.
- 🎥 **Video Call** – Jitsi Meet-based live video chat for shared meals.

---

## 🛠️ Tech Stack

| Layer      | Technologies                         |
|------------|--------------------------------------|
| Frontend   | React.js, Redux, Bootstrap           |
| Backend    | ASP.NET Core (.NET 8), C#            |
| Database   | MySQL 8+                             |
| Real-Time  | SignalR                              |
| Video Call | Jitsi Meet API                       |
| Auth       | JWT, BCrypt, Secure OTP              |
| Hosting    | Localhost/Cloud-ready                |

---

## 🧪 Testing & Performance

- ✅ **Unit & Integration Tests** for APIs and payment flows
- ⚙️ **Performance Testing** simulating high-concurrency scenarios
- 🔐 **Security Testing** focused on JWT auth, data privacy, and OTP-based transactions

---

## 🖥️ System Architecture

- 🧱 **MVC + Microservices** – Clean separation of concerns for scalability
- 🔁 **SignalR Integration** – Enables seamless real-time interactions
- 🔐 **JWT-based Auth** – Ensures secure user sessions

---

## 📷 Screenshots & Demo

Screenshots of login, menu, cart sharing, and live video call sessions are available in the `/screenshots` folder.  
Live Demo: _Coming Soon_ ✨

---

## 🌱 Future Enhancements

- 🤖 AI-based Food Recommendations
- 🛍️ Restaurant Partnership Integration
- 🌐 Global Payment Gateway Support (Stripe, PayPal, UPI)

---

## 📚 References

Books:
- _Learning MySQL_, _Pro C# 9 with .NET Core_, _Learning React_

Journals:
- _Performance Optimization Techniques for MySQL_, _Efficient Memory Management in .NET_

---

## 📌 How to Run Locally

### Backend (.NET 9)

cd VideoDiningApp
dotnet restore
dotnet run

### Frontend (React)

cd VideoDiningUi
npm install
npm start

📍 Ensure MySQL is running locally or update your connection string in appsettings.json.


