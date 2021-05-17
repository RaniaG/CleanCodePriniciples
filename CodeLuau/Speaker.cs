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

            var validationError = ValidateRegistration();
            if(validationError != null)
                return new RegisterResponse(validationError);

            RegistrationFee = CalculateRegistrationFee();

            var speakerId = repository.SaveSpeaker(this);

            return new RegisterResponse(speakerId);
		}

        private int CalculateRegistrationFee()
        {
            //TODO: should call the database and get the corresponding registration fee based on age
            return 0;
        }

        private RegisterError? ValidateRegistration()
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
            var isQualifiedSpeaker = YearsOfExperience > 10 || HasBlog || Certifications.Count > 3 || Constants.PREFFERED_EMPLOYERS.Contains(Employer);

            if (!isQualifiedSpeaker)
            {
                string emailDomain = Email.Split('@').Last();
                isQualifiedSpeaker = !(Constants.OLD_EMAIL_DOMAINS.Contains(emailDomain) || Browser.Name == WebBrowser.BrowserName.InternetExplorer || Browser.MajorVersion < 9);
            }

            return isQualifiedSpeaker;

        }

        private bool IsSpeakerQualified()
        {
            if (YearsOfExperience > 10) return true;
            if (HasBlog) return true;
            if (Certifications.Count > 3) return true;

            if (Constants.PREFFERED_EMPLOYERS.Contains(Employer)) return true;

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
            foreach (var tech in Constants.UNSUPPORTED_TECHNOLOGIES)
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