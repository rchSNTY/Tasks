using DailyTask.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace DailyTask.Controllers
{
    public class TaskController : Controller
    {
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;

        // READ
        public ActionResult Index()
        {
            List<TaskModel> list = new List<TaskModel>();

            try
            {
                using (MySqlConnection con = new MySqlConnection(connStr))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM tbl_task", con);
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        list.Add(new TaskModel
                        {
                            id = int.Parse(reader["id"].ToString()),
                            Name = reader["Name"].ToString(),
                            Description = reader["Task"].ToString(),
                            Day = reader["Day"].ToString()
                        });
                    }
                }
            }
            catch (MySqlException ex)
            {
                ViewBag.Error = $"Database error: {ex.Message}";
            }

            return View(list);
        }

        // CREATE
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TaskModel task)
        {
            if (task == null)
            {
                ViewBag.Error = "Task data is missing.";
                return View();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Invalid task data.";
                return View(task);
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(connStr))
                {
                    con.Open();
                    string query = "INSERT INTO tbl_task (Name, Task, Day) VALUES (@Name, @Task, @Day)";
                    MySqlCommand cmd = new MySqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@Name", task.Name ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Task", task.Description ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Day", task.Day ?? string.Empty);


                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Task created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error creating task: {ex.Message}";
                return View(task);
            }
        }


        // EDIT
        public ActionResult Edit(int id)
        {
            TaskModel task = new TaskModel();

            try
            {
                using (MySqlConnection con = new MySqlConnection(connStr))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM tbl_task WHERE id=@id", con);
                    cmd.Parameters.AddWithValue("@id", id);
                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        task.id = id;
                        task.Name = reader["Name"].ToString();
                        task.Description = reader["Task"].ToString();
                        task.Day = reader["Day"].ToString();
                    }
                }
            }
            catch (MySqlException ex)
            {
                ViewBag.Error = $"Database error: {ex.Message}";
                return RedirectToAction("Index");
            }

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // ADD THIS LINE
        public ActionResult Edit(TaskModel task)
        {
            // ADD NULL CHECK
            if (task == null)
            {
                ViewBag.Error = "Task data is null";
                return RedirectToAction("Index");
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(connStr))
                {
                    con.Open();
                    string query = "UPDATE tbl_task SET Name=@Name, Task=@Task, Day=@Day WHERE id=@id";
                    MySqlCommand cmd = new MySqlCommand(query, con);

                    // USE NULL COALESCING
                    cmd.Parameters.AddWithValue("@Name", task.Name ?? "");
                    cmd.Parameters.AddWithValue("@Task", task.Description ?? "");
                    cmd.Parameters.AddWithValue("@Day", task.Day ?? "");
                    cmd.Parameters.AddWithValue("@id", task.id);

                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Task updated successfully!";
                return RedirectToAction("Index");
            }
            catch (MySqlException ex)
            {
                ViewBag.Error = $"Database error: {ex.Message}";
                return View(task);
            }
        }

        // DELETE
        public ActionResult Delete(int id)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connStr))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("DELETE FROM tbl_task WHERE id=@id", con);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Task deleted successfully!";
            }
            catch (MySqlException ex)
            {
                TempData["ErrorMessage"] = $"Error deleting task: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}