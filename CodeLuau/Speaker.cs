using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeLuau
{
	/// <summary>
	/// Represents a single speaker
	/// </summary>
	public class Speaker
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public int? YearsOfExperience { get; set; }
		public bool HasBlog { get; set; }
		public string BlogURL { get; set; }
		public WebBrowser Browser { get; set; }
		public List<string> Certifications { get; set; }
		public string Employer { get; set; }
		public int RegistrationFee { get; set; }
		public List<Session> Sessions { get; set; }

		/// <summary>
		/// Register a speaker
		/// </summary>
		/// <returns>speakerID</returns>
		public RegisterResponse Register(IRepository repository)
		{
			int? speakerId = null;

            var validationError = ValidateData();
            if(validationError != null)
                return new RegisterResponse(validationError);

            if (YearsOfExperience <= 1)
            {
                RegistrationFee = 500;
            }
            else if (YearsOfExperience >= 2 && YearsOfExperience <= 3)
            {
                RegistrationFee = 250;
            }
            else if (YearsOfExperience >= 4 && YearsOfExperience <= 5)
            {
                RegistrationFee = 100;
            }
            else if (YearsOfExperience >= 6 && YearsOfExperience <= 9)
            {
                RegistrationFee = 50;
            }
            else
            {
                RegistrationFee = 0;
            }

            try
            {
                speakerId = repository.SaveSpeaker(this);
            }
            catch (Exception e)
            {
            }

            return new RegisterResponse((int)speakerId);
		}

        private RegisterError? ValidateData()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
                return RegisterError.FirstNameRequired;

            if (string.IsNullOrWhiteSpace(LastName))
                return RegisterError.LastNameRequired;

            if (string.IsNullOrWhiteSpace(Email))
                return RegisterError.EmailRequired;

            if(!Sessions.Any())
                return RegisterError.NoSessionsProvided;

            if (!IsSpeakerApproved())
                return RegisterError.SpeakerDoesNotMeetStandards;

            if (!IsAnySessionApproved())
                return RegisterError.NoSessionsApproved;

            return null;
        }

        private bool IsSpeakerApproved()
        {
            var employers = new List<string>() { "Pluralsight", "Microsoft", "Google" };
            var isQualifiedSpeaker = YearsOfExperience > 10 || HasBlog || Certifications.Count > 3 || employers.Contains(Employer);

            if (!isQualifiedSpeaker)
            {
                string emailDomain = Email.Split('@').Last();
                var oldEmailDomains = new List<string>() { "aol.com", "prodigy.com", "compuserve.com" };

                isQualifiedSpeaker = !(oldEmailDomains.Contains(emailDomain) || Browser.Name == WebBrowser.BrowserName.InternetExplorer || Browser.MajorVersion < 9);
            }

            return isQualifiedSpeaker;

        }

        private bool IsSpeakerQualified()
        {
            if (YearsOfExperience > 10) return true;
            if (HasBlog) return true;
            if (Certifications.Count > 3) return true;

            var employers = new List<string>() { "Pluralsight", "Microsoft", "Google" };
            if (employers.Contains(Employer)) return true;

            return false;
        }

        private bool IsAnySessionApproved()
        {
            foreach (var session in Sessions)
            {
                session.Approved = !IsSessionOldTechnology(session);
            }
            return Sessions.Any(e=>e.Approved);
        }

        private bool IsSessionOldTechnology(Session session)
        {
            var oldTechnologies = new List<string>() { "Cobol", "Punch Cards", "Commodore", "VBScript" };

            foreach (var tech in oldTechnologies)
            {
                if (session.Title.Contains(tech) || session.Description.Contains(tech))
                {
                    return true;
                }
            }
            return false;
        }
    }
}