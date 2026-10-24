using WService.Model;

namespace WService.DataSimulation {
    public class StudentDataSimulation {

        // we will simulate database where we will strore and handle data in memory not in db!
        public static readonly List<Student> studentList = new List<Student>{
            new Student { Id= 1, Name="Ahmed",Age=23,Grade=80},
            new Student { Id= 2, Name="Mohammed",Age=24,Grade=99},
            new Student { Id= 3, Name="Osama",Age=22,Grade=75},
            new Student { Id= 4, Name="Ali",Age=20,Grade=80},
            new Student { Id= 5, Name="Wesame",Age=24,Grade=40},
            new Student { Id= 6, Name="Waseem",Age=22,Grade=30}

        };

    }
}
