﻿namespace CV_Applikation.Models
{
    public class EditProfileViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Adress { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePicture { get; set; }
        public bool IsPrivate { get; set; }

    }
}