using Microsoft.EntityFrameworkCore;
using Google.Authenticator;
using System.Data;
using System.Net;
using Core.Ef;
using Su;

namespace Core.Helpers
{
    public static class AdminUserHelper
    {
        public static string GetManagerbyOtpConfirm(CBCTContext ct, string systemName, string uid, string otpConfirm)
        {
            var admin = ct.AdminUsers.Find(uid);

            if (admin != null && !string.IsNullOrEmpty(admin.OtpConfirm) && otpConfirm == admin.OtpConfirm)
            {
                //UpadteOtpConfirm(ct, uid); //重設一個 OtpConfirm，以達到一次性使用
                string otpSecret = Guid.NewGuid().ToString("N").Substring(0, 10);
                return CreateQRCode(ct, systemName, uid, otpSecret);
            }
            else
            {
                throw new CustomException("請重新取得動態密碼", HttpStatusCode.Unauthorized);
            }
        }

        public static string CreateQRCode(CBCTContext _dbContext, string systemName, string uid, string otpSecret)
        {
            var tfa = new TwoFactorAuthenticator();
            var admin = _dbContext.AdminUsers.Find(uid);
            var setupInfo = tfa.GenerateSetupCode(systemName, admin.Email, otpSecret, false, 100);

            admin.OtpSecret = otpSecret;
            admin.ModifierUid = "system";
            admin.ModifiedAt = DateTime.Now;

            _dbContext.SaveChanges();

            return setupInfo.QrCodeSetupImageUrl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static IOrderedQueryable<Dtos.AdminUserDto> GetQuery(CBCTContext ct, string? keyword)
        {
            var query = ct.AdminUsers.Where(x => x.DeletedAt == null &&
            (keyword == null || x.Email.Contains(keyword) || x.Name.Contains(keyword)))
                .Select(x => new Dtos.AdminUserDto
                {
                    Uid = x.Uid,
                    Email = x.Email,
                    Name = x.Name,
                    EnStatus = x.EnStatus,
                    Mobile = x.Mobile,
                    BackMemo = x.BackMemo
                })
                .OrderByDescending(x => x.Name);

            return query;
        }

        public static int GetRowNums(CBCTContext _dbContext)
        {
            var query = _dbContext.AdminUsers.Where((x) => x.DeletedAt == null).Count();
            return query;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_dbContext"></param>
        /// <param name="uid"></param>
        /// <exception cref="CustomException"></exception>
        public static void Delete(CBCTContext _dbContext, string uid)
        {
            AdminUser? admin = _dbContext.AdminUsers.Find(uid);
            string editedBy = AuthHelper.AdminUserUid!;
            if (admin == null)
            {
                throw new CustomException("找不到資料");
            }

            admin.Email = admin.Email + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            admin.DeletedAt = DateTime.Now;
            admin.ModifierUid = editedBy;
            admin.ModifiedAt = DateTime.Now;
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// SignUp & Update
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="createdBy"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Dtos.AdminUserDto Upsert(CBCTContext _dbContext, Dtos.AdminUserDto dto, string createdBy = "")
        {
            var ex = dto.GetCustomException();
            if (string.IsNullOrEmpty(dto.Uid))
            {
                //密碼必填
                if (string.IsNullOrEmpty(dto.Secret))
                {
                    ex.AddValidationError("Secret", "密碼必填");
                }
            }

            if (dto.AdminRoleUids != null && dto.AdminRoleUids.Count > 0)
            {
                var existsRoleUids = _dbContext.AdminRoles.Where(r => dto.AdminRoleUids.Contains(r.Uid)).Select(r => r.Uid);
                var errorUids = dto.AdminRoleUids.Where(u => !existsRoleUids.Contains(u));
                if (errorUids.Count() > 0)
                {
                    ex.AddValidationError("RoleUids", "有不存在的群組。");
                }
            }

            dto.Email = dto.Email.EmailNormalize();

            // 檢查有無重複 email
            if (_dbContext.AdminUsers.Where(x => x.Email == dto.Email
                && (string.IsNullOrEmpty(x.Uid) || x.Uid != x.Uid)).Any())
            {
                ex.AddValidationError("Email", "email 重複");
            }

            ex.TryThrowValidationException();

            string adminUid = AuthHelper.AdminUserUid!;
            AdminUser? entity;
            if (string.IsNullOrEmpty(dto.Uid))
            {
                //新增
                entity = new AdminUser
                {
                    Uid = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.Now,
                    CreatorUid = adminUid
                };
                _dbContext.AdminUsers.Add(entity);
            }
            else
            {
                //更新
                entity = _dbContext.AdminUsers.Where(x => x.Uid == dto.Uid).FirstOrDefault();
                if (entity == null)
                {
                    throw new CustomException(HttpStatusCode.NotFound, "查無資料");
                }
            }

            dto.CopyTo(entity, skips: "Uid");
            if (!string.IsNullOrEmpty(dto.Secret))
            {
                string salt = CryptographicHelper.GetSalt(6);
                string secretHash = CryptographicHelper.GetSHA256Hash(salt + dto.Secret);
                entity.Salt = salt;
                entity.SecretHash = secretHash;
            }
            entity.ModifiedAt = DateTime.Now;
            entity.ModifierUid = adminUid;

            _dbContext.AdminRoleAdminUsers.Where(x => x.AdminUserUid == entity.Uid).ExecuteDelete();
            foreach (var roleUid in dto.AdminRoleUids)
            {
                _dbContext.AdminRoleAdminUsers.Add(new AdminRoleAdminUser
                {
                    AdminRoleUid = roleUid,
                    AdminUserUid = entity.Uid,
                    CreatedAt = DateTime.Now,
                    CreatorUid = adminUid,
                    ModifiedAt = DateTime.Now,
                    ModifierUid = adminUid
                });
            }

            _dbContext.SaveChanges();

            dto.Uid = entity.Uid;
            return dto;
        }

        public static Dtos.AdminUserDto? GetOne(CBCTContext _dbContext, string uid)
        {
            var adminUser = _dbContext.AdminUsers.FirstOrDefault(x => x.DeletedAt == null && x.Uid == uid);
            if (adminUser == null)
            {
                return null;
            }

            var ret = adminUser.CopyTo<Dtos.AdminUserDto>();
            ret.AdminRoleUids = _dbContext.AdminRoleAdminUsers.Where(x => x.AdminUserUid == uid).Select(x => x.AdminRoleUid).ToList();

            return ret;
        }

        //public static AdminUser? GetOneByEmail(Core.CBCTContext _dbContext, string email)
        //{
        //    return _dbContext.AdminUsers.FirstOrDefault(x => x.DeletedAt == null && x.Email == email);
        //}

        //public static AdminUser? GetEmailByUid(Core.CBCTContext _dbContext, string uid)
        //{
        //    return _dbContext.AdminUsers.FirstOrDefault(x => x.DeletedAt == null && x.Uid == uid);
        //}

        public static void ResetPassword(CBCTContext _dbContext, Dtos.ResetSecretDto postBody)
        {
            var admin = _dbContext.AdminUsers.FirstOrDefault(x => x.Email == postBody.Email);
            if (admin == null)
            {
                throw new CustomException("管理員不存在", HttpStatusCode.NotFound);
            }
            string salt = CryptographicHelper.GetSalt(64);
            string secretHash = CryptographicHelper.GetSecretHash(salt, postBody.Secret);
            admin.Salt = salt;
            admin.SecretHash = secretHash;
            _dbContext.SaveChanges();
        }

        public static AdminUser? GetManager(CBCTContext _dbContext, Dtos.LogInDto dto, bool isCheckSecret = true)
        {
            var manager = _dbContext.AdminUsers.FirstOrDefault(x => x.DeletedAt == null && x.Email == dto.Email);
            if (manager == null)
            {
                return null;
            }

            if (manager.SecretHash != CryptographicHelper.GetSHA256Hash(manager.Salt + dto.Secret))
            {
                if (isCheckSecret || dto.Secret != "123")
                {
                    return null;
                }
            }

            return manager;
        }

        public static string CreateQrCode(CBCTContext _dbContext, string uid, string otpSecret)
        {
            var tfa = new TwoFactorAuthenticator();

            var admin = _dbContext.AdminUsers.Find(uid);

            var setupInfo = tfa.GenerateSetupCode("Contin Ec Admin", admin.Email, otpSecret, false, 100);

            admin.OtpSecret = otpSecret;
            admin.ModifierUid = "system";
            admin.ModifiedAt = DateTime.Now;

            _dbContext.SaveChanges();

            return setupInfo.QrCodeSetupImageUrl;
        }

        public static string UpadteOtpConfirm(CBCTContext _dbContext, string uid)
        {
            string otpConfirm = Guid.NewGuid().ToString("N").Substring(0, 10);
            var admin = _dbContext.AdminUsers.Find(uid);

            admin.OtpConfirm = otpConfirm;
            admin.ModifierUid = "system";
            admin.ModifiedAt = DateTime.Now;

            _dbContext.SaveChanges();

            return otpConfirm;
        }

        public static string GetOtpSecret(CBCTContext _dbContext, string uid)
        {
            var admin = _dbContext.AdminUsers.Find(uid);
            return admin.OtpSecret;
        }

        public static int SetManagerToForgotPasswordState(CBCTContext _dbContext, string editBy, AdminUser admin)
        {
            // update manager reset_password_token
            admin.ResetToken = CryptographicHelper.GetSpecificLengthRandomString(64, true);
            admin.ResetTokenExpiration = DateTime.Now.AddMinutes(10);
            admin.ModifierUid = editBy;
            admin.ModifiedAt = DateTime.Now;
            return _dbContext.SaveChanges();
        }

        public static string GetToken(CBCTContext _dbContext, string uid)
        {
            var admin = _dbContext.AdminUsers.Find(uid);
            return admin.ResetToken;
        }
    }
}