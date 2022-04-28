using System;
using System.Collections.Generic;
using System.Text;

namespace AntiEpos
{
    public class AEUserInfo
    {
        /// <summary>
        /// ...????????
        /// </summary>
        public ulong AId { get; set; }

        /// <summary>
        /// айди профиля в эпос
        /// </summary>
        public ulong ProfileId { get; set; }

        /// <summary>
        /// Вот этот токен вроде как и используется клиентом доски для авторизации
        /// </summary>
        public string? AuthToken { get; set; }

        /// <summary>
        /// Сессии пользователя
        /// </summary>
        public AESessionsData? Sessions { get; set; }

        /// <summary>
        /// Академические года
        /// </summary>
        public AEAcademicYear[]? AcademicYears { get; set; }
    }
}
