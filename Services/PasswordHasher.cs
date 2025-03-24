using System;
using BCrypt.Net;

public class PasswordHasher
{
    public static void HashAndPrintPassword(string password)
    {
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        Console.WriteLine("Hashed Password: " + hashedPassword);
    }
}
