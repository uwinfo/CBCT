using Microsoft.EntityFrameworkCore;
using Google.Authenticator;
using System.Data;
using System.Net;
using Core.Ef;
using Su;

namespace Core.Helpers
{
    public static class DoctorHelper
    {

        public static Ef.Doctor? GetOne(CBCTContext _dbContext, string uid)
        {
            return _dbContext.Doctors.FirstOrDefault(x => x.DeletedAt == null && x.Uid == uid);
        }
    }
}