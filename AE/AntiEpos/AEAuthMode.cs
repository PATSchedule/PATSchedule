using System;
using System.Collections.Generic;
using System.Text;

namespace AntiEpos
{
    /// <summary>
    /// Режим аутентификации учётной записи ЭПОС.
    /// </summary>
    public class AEAuthMode
    {
        /// <summary>
        /// Режим как query строка в HTTP запросе
        /// </summary>
        public string Mode { get; set; }

        private AEAuthMode(string privateauthmodehttp)
        {
            Mode = privateauthmodehttp;
        }

        /// <summary>
        /// Учитель, форма авторизации - министерства развития и связи пермского края.
        /// </summary>
        public static AEAuthMode Teacher = new AEAuthMode("rsaa");

        /// <summary>
        /// Ученик, форма авторизации - РСААГ (красивая).
        /// </summary>
        public static AEAuthMode Student = new AEAuthMode("rsaags");

        /// <summary>
        /// Родитель ученика, форма авторизации - РСААГ (красивая + выбор детей).
        /// </summary>
        public static AEAuthMode Parent = new AEAuthMode("rsaag");

        /// <summary>
        /// Возвращает режим авторизации как HTTP параметр
        /// </summary>
        /// <returns>см. выше</returns>
        public override string ToString()
        {
            return Mode;
        }
    }
}
