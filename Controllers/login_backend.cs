using Microsoft.Data.SqlClient;

namespace RaceDayAPI.Controllers
{
    public class login_backend
    {
       
            //start of login_backend class

            //creating a method to check if the user exists in the database
            //using SqlConnection and SqlCommand, verify the password,
            //start a session and return the user's details

        public string login_user(string login_email, string login_password, HttpContext httpContext)
        {//start of login_user method

            //variable declaration to store the message and the connection string to the database
            string message = "Failed to login, try again later...";
            string connectionString = @"Data source=(localdb)\part_2_api;database=raceday2;";

            //try and catch
            try
            {//start of try statement

                //creating an instance for the SqlConnection class
                //with an object name called connect
                SqlConnection connect = new SqlConnection(connectionString);

                //open the connection to the database
                connect.Open();

                //query to get the user's information from the database using the email
                string query = @"select id, fullname, email, role, password from participants where email = @email";

                //creating an instance for the SqlCommand class
                //with an object name called run_query
                SqlCommand run_query = new SqlCommand(query, connect);

                //add the email as a parameter to avoid sql injection
                run_query.Parameters.AddWithValue("@email", login_email);

                //run/execute the query and read the results
                SqlDataReader reader = run_query.ExecuteReader();

                //check if the user exists in the database
                if (reader.Read())
                {//start of if statement

                    //get the user's details from the database
                    int id = Convert.ToInt32(reader["id"]);
                    string fullname = reader["fullname"].ToString() ??"";
                    string email = reader["email"].ToString() ??"";
                    string role = reader["role"].ToString() ??"";
                    string hashedPassword = reader["password"].ToString() ??"";

                    //check if the password entered matches the hashed password
                    //using BCrypt.Net.BCrypt.Verify method
                    bool passwordMatches = BCrypt.Net.BCrypt.Verify(login_password, hashedPassword);

                    if (passwordMatches)
                    {//start of if statement

                        //store the user's authenticated state, id and role in the session
                        //so that they can be used for all subsequent requests
                        httpContext.Session.SetInt32("user_id", id);
                        httpContext.Session.SetString("user_email", email);
                        httpContext.Session.SetString("user_fullname", fullname);
                        httpContext.Session.SetString("user_role", role);

                        //return the user's details
                        message = "Login successful. Welcome " + fullname +
                                  " | ID: " + id +
                                  " | Email: " + email +
                                  " | Role: " + role;

                    }//end of if statement
                    else
                    {//start of else statement

                        //password is incorrect
                        message = "Invalid email or password.";

                    }//end of else statement

                }//end of if statement
                else
                {//start of else statement

                    //user does not exist in the database
                    message = "Invalid email or password.";

                }//end of else statement

                //close the reader and the connection to the database
                reader.Close();
                connect.Close();

            }// end of try statement
            catch (Exception error)
            {//start of catch statement

                //return error message
                //assign first
                message = error.Message;

            }//end of catch statement

            //return message
            return message;

        }//end of login_user method
    }
    }
