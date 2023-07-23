using System;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: dotnet script.exe <username> <password>");
            return;
        }

        string newUsername = args[0];
        string newPassword = args[1];

        CreateUser(newUsername, newPassword);
        SetupSSH(newUsername);
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

    static void SetupSSH(string username)
    {
        try
        {
            Process.Start("mkdir", $"/home/{username}/.ssh").WaitForExit();
            Process.Start("chmod", $"700 /home/{username}/.ssh").WaitForExit();
            Process.Start("ssh-keygen", $"-t rsa -f /home/{username}/.ssh/id_rsa -N ''").WaitForExit();
            Process.Start("chown", $"{username}:{username} -R /home/{username}/.ssh").WaitForExit();
            Console.WriteLine($"SSH keys generated and set up for user '{username}'.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to set up SSH keys for user '{username}': {e.Message}");
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