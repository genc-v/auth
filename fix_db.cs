using System;
using MySqlConnector;

class Program
{
    static void Main()
    {
        string connStr = "Server=localhost;Port=33063;Uid=user;Pwd=alskjdfa@alskdjfAAAb12;";
        using var conn = new MySqlConnection(connStr);
        conn.Open();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            DROP DATABASE IF EXISTS `auth`;
            CREATE DATABASE `auth`;
        ";
        cmd.ExecuteNonQuery();
        Console.WriteLine("Database dropped and recreated.");
    }
}
