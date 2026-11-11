using BCrypt.Net;
using WService.Model;

namespace WService.DataSimulation {
    public class StudentDataSimulation {

        // we will simulate database where we will store and handle data in memory not in db!
        public static readonly List<Student> studentList = new List<Student>{
            new Student { Id= 1, Name="Ahmed",Age=23,Grade=80,
                Email="ahmed@gmail.com",
                HashedPassword=BCrypt.Net.BCrypt.HashPassword("ahmedpass12") ,
                Role="Admin"},
            new Student { Id= 2, Name="Mohammed",Age=24,Grade=99,
                Email="mohammed@gmail.com",
                HashedPassword=BCrypt.Net.BCrypt.HashPassword("mhpass12") ,
                Role="Student"},
            new Student { Id= 3, Name="Osama",Age=22,Grade=75,
                Email="osama@gmail.com",
                HashedPassword=BCrypt.Net.BCrypt.HashPassword("ospass12"),
                Role="Student"},
            new Student { Id= 4, Name="Ali",Age=20,Grade=80,
                Email="ali@gmail.com",
                HashedPassword=BCrypt.Net.BCrypt.HashPassword("alipass12"),
                Role="Student"},
            new Student { Id= 5, Name="Wessame",Age=24,Grade=40,
                Email="ws@gmail.com",
                HashedPassword=BCrypt.Net.BCrypt.HashPassword("wwpass12"),
                Role="Student"},
            new Student { Id= 6, Name="Waseem",Age=22,Grade=30,
                Email="wm@gmail.com",
                HashedPassword=BCrypt.Net.BCrypt.HashPassword("wwpass12"),
                Role="Admin"}
        };
    }
}