using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OnlineStudents.Web.Models;

namespace OnlineStudents.Web.Controllers
{
    [Route("api/Canvas")]
    [ApiController]
    public class CanvasController : Controller
    {
        public IConfiguration Configuration { get; set; }

        public CanvasController(IConfiguration config)
        {
            Configuration = config;
        }

        [HttpGet("GetAllTerms")]
        public ActionResult GetAllTerms()
        {
            var result = new List<EnrollmentTermDTO>();

            var conn = new NpgsqlConnection(Configuration.GetConnectionString("redshiftConnectionString"));

            try
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;

                    cmd.CommandText = "SELECT * from enrollment_term_dim";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var schema = reader.GetColumnSchema();
                            var enrollmentTerm = new EnrollmentTermDTO();
                            if (DateTime.TryParse(reader["date_start"].ToString(), out DateTime startDateValue))
                            {
                                enrollmentTerm.StartDate = startDateValue;
                            }

                            if (DateTime.TryParse(reader["date_end"].ToString(), out DateTime endDateValue))
                            {
                                enrollmentTerm.EndDate = endDateValue;
                            }

                            enrollmentTerm.Id = reader["id"].ToString();
                            enrollmentTerm.CanvasId = long.Parse(reader["canvas_id"].ToString());
                            enrollmentTerm.RootAccountId = long.Parse(reader["root_account_id"].ToString());
                            enrollmentTerm.Name = reader["name"].ToString();

                            enrollmentTerm.SisSourceId = reader["sis_source_id"].ToString();

                            result.Add(enrollmentTerm);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            finally
            {
                if (conn.FullState == ConnectionState.Open)
                    conn.Close();

                conn.Dispose();
            }

            return Ok(result.Where(x => x.EndDate != null || x.Name.ToLower().Contains("migration") || x.Name.ToLower().Contains("default")).OrderBy(x => x.EndDate));
        }

        [HttpPost("GetReport")]
        public ActionResult GetReport([FromBody] ReportOptions reportOptions)
        {
            var results = new List<string>();
            var commandSb = new StringBuilder();

            commandSb.Append("SELECT DISTINCT communication_channel_dim.address email FROM course_dim ");

            commandSb.Append("JOIN enrollment_dim ON enrollment_dim.course_id = course_dim.id ");
            commandSb.Append("JOIN user_dim ON enrollment_dim.user_id = user_dim.id ");
            commandSb.Append("JOIN communication_channel_dim ON communication_channel_dim.user_id = user_dim.id ");

            if (reportOptions.Restrict)
            {
                commandSb.Append("LEFT JOIN requests ON requests.user_id = user_dim.id ");
                commandSb.Append("AND requests.course_id = course_dim.id ");
            }

            commandSb.Append($"WHERE course_dim.enrollment_term_id = '{reportOptions.Term}' ");
            commandSb.Append("AND enrollment_dim.workflow_state = 'active' ");
            commandSb.Append("AND enrollment_dim.type = 'StudentEnrollment' ");
            commandSb.Append("AND communication_channel_dim.type = 'email' ");

            switch (reportOptions.Type)
            {
                case "o":
                    commandSb.Append("AND course_dim.code ~ 'E0[0-9]' ");
                    break;
                case "h":
                    commandSb.Append("AND course_dim.code ~ 'H0[0-9]' ");
                    break;
                case "b":
                    commandSb.Append("AND course_dim.code ~ '(E|H)0[0-9]' ");
                    break;
            }


            if (reportOptions.Restrict)
            {
                commandSb.Append("GROUP BY email HAVING COUNT(requests.user_id) = 0;");
            }

            var conn = new NpgsqlConnection(Configuration.GetConnectionString("redshiftConnectionString"));

            try
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandTimeout = 0;

                    cmd.CommandText = commandSb.ToString();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(reader["email"].ToString());
                        }
                    }
                }

                results = results.GroupBy(x => x).Select(x => x.First()).ToList();

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            finally
            {
                if (conn.FullState == ConnectionState.Open)
                    conn.Close();

                conn.Dispose();
            }

            return Ok(results);
        }

        public class ReportOptions
        {
            public string Term { get; set; }
            public string Type { get; set; }
            public bool Restrict { get; set; }
            public bool Cached { get; set; }
        }
    }
}