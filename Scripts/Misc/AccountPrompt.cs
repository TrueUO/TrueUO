using Server.Accounting;
using System;

namespace Server.Misc
{
    public class AccountPrompt
    {
        public static void Initialize()
        {
            if (Accounts.Count == 0 && !Core.Service)
            {
                Console.WriteLine("Containered:" + Environment.GetEnvironmentVariable("Containered"));
                if (Environment.GetEnvironmentVariable("Containered") == "1")
                {
                    Console.WriteLine("Creating a default admin account");
                    const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    char[] randomCharSet = new char[10];
                    Random rnd = new Random();

                    for (int i = 0; i < 10; i++)
                    {
                        randomCharSet[i] = chars[rnd.Next(chars.Length)];
                    }

                    string tempToken = new string(randomCharSet);

                    Console.WriteLine($"Username: admin \n " +
                        $"Password: {tempToken}\n " +
                        $"Please ensure this is changed.");
                    _ = new Account("admin", tempToken)
                    {
                        AccessLevel = AccessLevel.Owner
                    };
                    return;
                }

                if (Environment.UserInteractive)
                {
                    Console.WriteLine("This server has no accounts.");
                    Console.WriteLine("Do you want to create the owner account now? (y/n)");

                    string key = Console.ReadLine();

                    if (key != null && key.ToUpper() == "Y")
                    {
                        Console.WriteLine();

                        Console.WriteLine("Username: ");
                        string username = Console.ReadLine();

                        Console.WriteLine("Password: ");
                        string password = Console.ReadLine();

                        _ = new Account(username, password)
                        {
                            AccessLevel = AccessLevel.Owner
                        };

                        Console.WriteLine("Account created.");
                    }
                    else
                    {
                        Console.WriteLine();

                        Console.WriteLine("Account not created.");
                    }
                }
            }
        }
    }
}
