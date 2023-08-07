using System;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1 || args.Length > 2)
        {
            Console.WriteLine("Usage: dotnet script.exe <username> [password]");
            return;
        }

        string newUsername = args[0];
        string newPassword = args.Length == 2 ? args[1] : null;

        if (string.IsNullOrEmpty(newPassword))
        {
            CreateUserWithoutPassword(newUsername);
        }
        else
        {
            CreateUser(newUsername, newPassword);
        }

        SetupSSH(newUsername);
        AddPublicKeyToAuthorizedKeys(newUsername);
        ChangeDefaultShell(newUsername, "/bin/bash"); // Change the shell to BASH
        GrantSudoPrivileges(newUsername);
        GrantPasswordlessSudoAccess(newUsername);
    }

    static void CreateUser(string username, string password)
    {
        try
        {
            Process.Start("useradd", $"-m -p {password} {username}").WaitForExit();
            Console.WriteLine($"User '{username}' created successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to create user '{username}': {e.Message}");
        }
    }

    static void CreateUserWithoutPassword(string username)
    {
        try
        {
            Process.Start("useradd", $"-m -s /usr/sbin/nologin {username}").WaitForExit();
            Console.WriteLine($"User '{username}' created successfully without a password.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to create user '{username}': {e.Message}");
        }
    }

    static void SetupSSH(string username)
    {
        try
        {
            Process.Start("mkdir", $"/home/{username}/.ssh").WaitForExit();
            Process.Start("chmod", $"700 /home/{username}/.ssh").WaitForExit();
            Process.Start("ssh-keygen", $"-t ed25519 -f /home/{username}/.ssh/id_ed25519 -N ''").WaitForExit();
            Process.Start("chown", $"{username}:{username} -R /home/{username}/.ssh").WaitForExit();
            Console.WriteLine($"ED25519 SSH keys generated and set up for user '{username}'.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to set up ED25519 SSH keys for user '{username}': {e.Message}");
        }
    }

    static void AddPublicKeyToAuthorizedKeys(string username)
    {
        try
        {
            string publicKeyPath = $"/home/{username}/.ssh/id_ed25519.pub";
            string authorizedKeysPath = $"/home/{username}/.ssh/authorized_keys";

            if (!File.Exists(publicKeyPath))
            {
                Console.WriteLine("Public key file not found.");
                return;
            }

            string publicKey = File.ReadAllText(publicKeyPath);
            File.AppendAllText(authorizedKeysPath, publicKey);
            Console.WriteLine("Public key added to authorized_keys file.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to add public key to authorized_keys file: {e.Message}");
        }
    }

    static void ChangeDefaultShell(string username, string shellPath)
    {
        try
        {
            Process.Start("usermod", $"-s {shellPath} {username}").WaitForExit();
            Console.WriteLine($"Default shell changed to '{shellPath}' for user '{username}'.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to change default shell for user '{username}': {e.Message}");
        }
    }

    static void GrantSudoPrivileges(string username)
    {
        try
        {
            Process.Start("usermod", $"-aG sudo {username}").WaitForExit();
            Console.WriteLine($"Sudo privileges granted to user '{username}'.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to grant sudo privileges to user '{username}': {e.Message}");
        }
    }

    static void GrantPasswordlessSudoAccess(string username)
    {
        try
        {
            // Edit the sudoers file to grant passwordless sudo access to the user.
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "sudo";
                process.StartInfo.Arguments = $"visudo -f /etc/sudoers -c -q -s -A -V -i -p '' -u {username} -r {username}";
                process.Start();
                process.WaitForExit();
            }

            Console.WriteLine($"Passwordless sudo access granted to user '{username}'.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to grant passwordless sudo access to user '{username}': {e.Message}");
        }
    }
}