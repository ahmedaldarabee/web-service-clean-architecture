using StudentAPIDataAccessLayer;

namespace StudentAPIBusinessLayer {
    public class Student {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int Grade { get; set; }

        public Student(StudentDTO SDTO) {
            this.Id = SDTO.Id;
            this.Name = SDTO.Name;
            this.Age = SDTO.Age;
            this.Grade = SDTO.Grade;
        }

        public StudentDTO SDTO {
            get {return (new StudentDTO(this.Id,this.Name,this.Age,this.Grade));}
        }

        public static List<StudentDTO> GetAllStudents(){
            return StudentData.GetAllStudent();
        }
        
        public static List<StudentDTO> GetPassedStudent() {
            return StudentData.GetPassedStudent();
        }

        public static double GetStudentAverage() {
            return StudentData.GetStudentAverage();
        }

        public static StudentDTO GetStudentByID(int id){
            return StudentData.GetStudentByID(id);
        }

        public static int AddStudent(StudentDTO student) {
            return StudentData.AddNewStudent(student);
        }
        public static bool UpdateStudent(StudentDTO student)
        {
            return StudentData.UpdateStudent(student);
        }
        public static bool DeleteStudent(int id) {
            return StudentData.DeleteStudent(id);
        }
    }
}
