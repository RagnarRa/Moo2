using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net.Http;

namespace WaitingListTester
{
	class Student
	{
		public string SSN { get; set; }
		public string Name { get; set; }
	}

	class Program
	{


		private static Dictionary<int, bool> _rulesImplemented;

		static void Main(string[] args)
		{
			_rulesImplemented = new Dictionary<int, bool>();

			var baseUrl = args[0];
			if (String.IsNullOrEmpty(baseUrl))
			{
				Console.WriteLine("Usage: WaitingListTester <baseUrl>");
				Console.WriteLine("Example: WaitingListTester http://localhost:port/api/courses");
				return;
			}

			RunAsync(baseUrl).Wait();

			ReportFinalResult();

			Console.Read();

		}

		static async Task RunAsync(string baseUrl)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				// Our test data:
				var course = new
				{
					TemplateID  = "T-514-VEFT",
					StartDate   = new DateTime(2016, 08, 18),
					EndDate     = new DateTime(2016, 11, 10),
					Semester    = "20163",
					MaxStudents = 4
				};
				var nonexistingStudent = new
				{
					SSN = "9876543210"
				};
				var student1 = new
				{
					SSN = "1234567890"
				};
				var student2 = new
				{
					SSN = "1234567891"
				};
				var student3 = new
				{
					SSN = "1234567892"
				};
				var student4 = new
				{
					SSN = "1234567893"
				};
				var student5 = new
				{
					SSN = "1234567894"
				};
				var student6 = new
				{
					SSN = "1234567895"
				};

				// 1. Create the course:
				var response = await client.PostAsJsonAsync(baseUrl, course);
				var courseResource = response.Headers.Location;
				ReportIntermedateResult("1. Creating course", response.StatusCode == HttpStatusCode.Created);

				// 2. Add a nonexisting student to the course:
				response = await client.PostAsJsonAsync(courseResource + "/students", nonexistingStudent);
				ReportResultForRule(0, response.StatusCode == HttpStatusCode.NotFound, "Should return 404");

				// 3: Add an existing student to the course:
				response = await client.PostAsJsonAsync(courseResource + "/students", student1);
				ReportIntermedateResult("3. Add student 1", response.StatusCode == HttpStatusCode.Created);

				// 4. Add an already enrolled student to the course
				response = await client.PostAsJsonAsync(courseResource + "/students", student1);
				ReportResultForRule(2, response.StatusCode == HttpStatusCode.PreconditionFailed, "Should return 412");

				// 5: Add an existing student to the course:
				response = await client.PostAsJsonAsync(courseResource + "/students", student2);
				ReportIntermedateResult("5. Add student 2", response.StatusCode == HttpStatusCode.Created);

				// 6: Add an existing student to the course:
				response = await client.PostAsJsonAsync(courseResource + "/students", student3);
				ReportIntermedateResult("6. Add student 3", response.StatusCode == HttpStatusCode.Created);

				// 7: Add an existing student to the course:
				response = await client.PostAsJsonAsync(courseResource + "/students", student4);
				ReportIntermedateResult("7. Add student 4", response.StatusCode == HttpStatusCode.Created);

				// 8. Add a student to a full course:
				response = await client.PostAsJsonAsync(courseResource + "/students", student5);
				ReportResultForRule(1, response.StatusCode == HttpStatusCode.PreconditionFailed, "Should return 412");

				// 9. Remove a student from the course:
				response = await client.DeleteAsync(courseResource + "/students/1234567890");
				ReportResultForRule(4, response.StatusCode == HttpStatusCode.NoContent, "Should return 204");
				// Technically this won't ensure that the record still exists...

				// 10. Verify that getting a list of all students won't
				//     include the one we just deleted:
				response = await client.GetAsync(courseResource + "/students");
				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsAsync<List<Student>>();
					ReportResultForRule(6, content.Count == 3, "Should contain 3 students");
				}
				else
				{
					ReportResultForRule(6, false, "Should return 200");
				}

				// 11. Ensure only existing students can be added to the waiting list:
				response = await client.PostAsJsonAsync(courseResource + "/waitinglist", nonexistingStudent);
				ReportResultForRule(0, response.StatusCode == HttpStatusCode.NotFound, "Should return 404");

				// 12. Add an existing person to the waiting list:
				response = await client.PostAsJsonAsync(courseResource + "/waitinglist", student1);
				ReportIntermedateResult("12. Add student to waiting list", response.StatusCode == HttpStatusCode.OK);

				// 13. Ensure waiting list contains the correct number of students:
				response = await client.GetAsync(courseResource + "/waitinglist");
				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsAsync<List<Student>>();
					ReportIntermedateResult("13. Get waiting list", response.StatusCode == HttpStatusCode.OK && content.Count == 1);
				}
				else
				{
					ReportIntermedateResult("13. Get waiting list", false);
				}

				// 14. Enroll student back to course:
				response = await client.PostAsJsonAsync(courseResource + "/students", student1);
				ReportResultForRule(5, response.StatusCode == HttpStatusCode.Created, "Should return 201");

				// 15. Ensure student on waiting list is no longer there
				//     after having been enrolled:
				response = await client.GetAsync(courseResource + "/waitinglist");
				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsAsync<List<Student>>();
					ReportResultForRule(3, response.StatusCode == HttpStatusCode.OK && content.Count == 0, "Should return 200 and waitinglist should now be empty");
				}
				else
				{
					ReportResultForRule(3, false, "Should return 200");
				}

				// 16. Add student to waiting list:
				response = await client.PostAsJsonAsync(courseResource + "/waitinglist", student6);
				ReportIntermedateResult("16. Add to waiting list", response.StatusCode == HttpStatusCode.OK);

				// 17. Try to add a student to the waiting list, while the student is already on the waiting list
				response = await client.PostAsJsonAsync(courseResource + "/waitinglist", student6);
				ReportResultForRule(7, response.StatusCode == HttpStatusCode.PreconditionFailed, "Should return 412");

				// 18. Try to add a student to the waiting list, already enrolled as a student
				response = await client.PostAsJsonAsync(courseResource + "/waitinglist", student2);
				ReportResultForRule(8, response.StatusCode == HttpStatusCode.PreconditionFailed, "Should return 412");

				// Finally, test that all the APIs will return 404
				// when an invalid course ID is supplied:

				response = await client.GetAsync(baseUrl + "9999999/students");
				ReportResultForRule(9, response.StatusCode == HttpStatusCode.NotFound, "Should return 404");

				response = await client.GetAsync(baseUrl + "9999999/waitinglist");
				ReportResultForRule(9, response.StatusCode == HttpStatusCode.NotFound, "Should return 404");

				response = await client.PostAsJsonAsync(baseUrl + "9999999/waitinglist", student5);
				ReportResultForRule(9, response.StatusCode == HttpStatusCode.NotFound, "Should return 404");

				response = await client.PostAsJsonAsync(baseUrl + "9999999/students", student5);
				ReportResultForRule(9, response.StatusCode == HttpStatusCode.NotFound, "Should return 404");

			}
		}

		static void ReportIntermedateResult(string item, bool success)
		{
			Console.Write(item);
			Console.Write(success ? " success" : " failure");
			Console.WriteLine();
		}

		static void ReportResultForRule(int ruleIndex, bool success, string err)
		{
			if (success)
			{
				Console.WriteLine("Rule" + ruleIndex + " success!");
			}
			else
			{
				Console.WriteLine("Rule" + ruleIndex + " failed! (" + err + ")");
			}

			// Only report that the rule has been implemented correctly
			// if it hasn't failed previously:
			if (_rulesImplemented.ContainsKey(ruleIndex))
			{
				if (!success)
				{
					_rulesImplemented[ruleIndex] = false;
				}
				// I.e. if the rule has been implemented previously,
				// but fails now, the failure overrides the previous success.
			}
			_rulesImplemented[ruleIndex] = success;
		}

		static void ReportFinalResult()
		{
			Console.WriteLine("============================");
			Console.WriteLine("Results:");

			var ordered = from x in _rulesImplemented
				orderby x.Key
				select x;

			foreach (var item in ordered)
			{
				Console.WriteLine("Rule" + item.Key + ":" + item.Value);
			}
		}
	}
}
