using Microsoft.Data.SqlClient;
using System.Data;

namespace StudentAPIDataAccessLayer {

    public class StudentDTO {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int Grade { get; set; }

        public StudentDTO(int id, string name, int age, int grade){
            this.Id = id;
            this.Name = name;
            this.Age = age;
            this.Grade = grade;
        }
    }
    
    public class StudentData{
        static string _connection = "Server=.;Database=StudentsDB;Integrated Security=True;TrustServerCertificate=True;";

        public static List<StudentDTO> GetAllStudent(){
            try {
                var studentList = new List<StudentDTO>();

                using (SqlConnection connection = new SqlConnection(_connection)) {
                    using (SqlCommand command = new SqlCommand("SP_GetAllStudent", connection)) {
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader()){
                            while (reader.Read()){
                                studentList.Add(new StudentDTO(
                                    reader.GetInt32(reader.GetOrdinal("Id")),
                                    reader.GetString(reader.GetOrdinal("Name")),
                                    reader.GetInt32(reader.GetOrdinal("Age")),
                                    reader.GetInt32(reader.GetOrdinal("Grade"))
                                ));
                            }
                            reader.Close();
                            //connection.Close();
                        }
                    }
                }

                return studentList;
            }
            catch{
                throw;
            }
        }

        public static List<StudentDTO> GetPassedStudent(){
            try {
                List<StudentDTO> studentList = new List<StudentDTO>();
                using (SqlConnection connection = new SqlConnection(_connection)){
                    using (SqlCommand command = new SqlCommand("SP_GetPassedStudent",connection)) {
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                studentList.Add(new StudentDTO(
                                    reader.GetInt32(reader.GetOrdinal("Id")),
                                    reader.GetString(reader.GetOrdinal("Name")),
                                    reader.GetInt32(reader.GetOrdinal("Age")),
                                    reader.GetInt32(reader.GetOrdinal("Grade"))
                                ));
                            }
                            reader.Close();
                        }
                        //connection.Close();
                    }
                }
                return studentList;
            }
            catch{
                throw;
            }
        } 
        
        public static double GetStudentAverage(){
            try {
                double avgResult;
                using (SqlConnection connection = new SqlConnection(_connection)){
                    using (SqlCommand command = new SqlCommand("SP_GetAVGStudent", connection)) {
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        object result = command.ExecuteScalar();

                        if (result != DBNull.Value) {
                            avgResult = Convert.ToDouble(result);
                        }
                        else {
                            avgResult = 0;
                        }
                        //connection.Close();
                    }
                }

                return avgResult;
            }
            catch {
                throw;
            }
        }
    
        public static bool IsExist(int Id){
            try {
                bool result = false;

                if (Id < 0) {
                    result = false;
                }

                using (SqlConnection connection = new SqlConnection(_connection))
                {
                    using (SqlCommand command = new SqlCommand("SP_IsExist", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = Id;

                        connection.Open();
                        object isExistResult = command.ExecuteScalar();
                        if (isExistResult != null && Convert.ToInt32(isExistResult) == 1)
                        {
                            result = true;
                        }
                        //connection.Close();
                    }
                }
                return result;
            }
            catch{
                throw;
            }
        }

        public static StudentDTO GetStudentByID(int Id) {
             try {
                if (!IsExist(Id)){
                    return null!;
                }
                StudentDTO student = null!;

                using (SqlConnection connection = new SqlConnection(_connection)) {
                    using (SqlCommand command = new SqlCommand("SP_GetStudentById", connection)) {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = Id;

                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader()){
                            while (reader.Read()){
                                student = (new StudentDTO(
                                    reader.GetInt32(reader.GetOrdinal("Id")),
                                    reader.GetString(reader.GetOrdinal("Name")),
                                    reader.GetInt32(reader.GetOrdinal("Age")),
                                    reader.GetInt32(reader.GetOrdinal("Grade"))
                                ));
                            }
                            reader.Close();
                            //connection.Close();
                        }
                    }
                }

                return student!;
            }
            catch{
                throw;
            }
        }
        
        public static int AddNewStudent(StudentDTO student){
            try {
                int savingResult = -1;
                if(student == null){
                    savingResult = -1;
                }
                using (SqlConnection connection = new SqlConnection(_connection)) {
                    using (SqlCommand command = new SqlCommand("SP_AddNewStudent", connection)) {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("@Name", SqlDbType.NVarChar).Value = student!.Name;
                        command.Parameters.Add("@Age", SqlDbType.Int).Value = student.Age;
                        command.Parameters.Add("@Grade", SqlDbType.Int).Value = student.Grade;

                        connection.Open();
                        object resultOfInsertion = command.ExecuteScalar();
                        
                        if(resultOfInsertion != null && int.TryParse(resultOfInsertion.ToString(), out int insertedId)) {
                            savingResult = insertedId;
                        }
                        connection.Close();
                    }
                }

                return savingResult!;
            }
            catch{
                throw;
            }
        }

        public static bool DeleteStudent(int Id){
            try {
                bool isDeleted = false;
                if (!IsExist(Id)) {
                    return isDeleted;
                }

                using (SqlConnection connection = new SqlConnection(_connection))
                {
                    using (SqlCommand command = new SqlCommand("SP_DeleteStudentById", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = Id;

                        connection.Open();
                        var effectedRow = command.ExecuteNonQuery();

                        if (effectedRow != 0) {
                            isDeleted = true;
                        }
                    }
                }

                return isDeleted!;
            }
            catch {
                throw;
            }
        }

        public static bool UpdateStudent(StudentDTO student) {
            try {
                bool updatingResult = false;
                if (student == null || !IsExist(student.Id)) {
                    return false;
                }

                using (SqlConnection connection = new SqlConnection(_connection)) {
                    using (SqlCommand command = new SqlCommand("SP_UpdateStudent", connection)) {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("@Id", SqlDbType.Int).Value = student.Id;
                        command.Parameters.Add("@Name", SqlDbType.NVarChar).Value = student.Name;
                        command.Parameters.Add("@Age", SqlDbType.Int).Value = student.Age;
                        command.Parameters.Add("@Grade", SqlDbType.Int).Value = student.Grade;

                        connection.Open();
                        object resultOfUpdating = command.ExecuteScalar();

                        if (resultOfUpdating != null && int.TryParse(resultOfUpdating.ToString(), out int updatedId)) {
                            updatingResult = true;
                        }
                    }
                }

                return updatingResult;
            }
            catch {
                throw;
            }
        }

    }
}
