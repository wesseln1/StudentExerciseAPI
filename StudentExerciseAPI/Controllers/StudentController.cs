using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StudentExerciseAPI.Models;

namespace StudentExerciseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {

        private readonly IConfiguration _config;


        public StudentController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection")  );
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, e.Name, e.Language, se.Id, se.ExerciseId, c.Name as CohortName
                                        FROM Student s 
                                        LEFT JOIN Cohort c ON s.CohortId = c.Id
                                        LEFT JOIN StudentExercise se ON s.Id = se.StudentId
                                        LEFT JOIN Exercise e ON e.Id = se.ExerciseId";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Student> allStudents = new List<Student>();

                    while (reader.Read())
                    {
                        var studentId = reader.GetInt32(reader.GetOrdinal("Id"));
                        var studentAlreadyAdded = allStudents.FirstOrDefault(s => s.Id == studentId);

                        if (studentAlreadyAdded == null)
                        {

                            Student aStudent = new Student
                            {
                                Id = studentId,
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                            };


                            var isInCohort = !reader.IsDBNull(reader.GetOrdinal("CohortId"));

                            if (isInCohort)
                            {
                                aStudent.Cohort = new Cohort()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                    Students = new List<Student>(),
                                    Instructors = new List<Instructor>()
                                };
                            }

                            //List<Exercise> allStudentsExercises = new List<Exercise>();
                            var exercise = !reader.IsDBNull(reader.GetOrdinal("ExerciseId"));
                            if (exercise)
                            {
                                var addExercise = new Exercise()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Language = reader.GetString(reader.GetOrdinal("Language"))
                                };

                                aStudent.Exercises.Add(addExercise);
                            }
                            allStudents.Add(aStudent);
                        }
                    };
                    reader.Close();

                    return Ok(allStudents);
                }
            }
        }
    }
}
