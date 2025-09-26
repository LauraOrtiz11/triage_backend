using System.Text.RegularExpressions;
using triage_backend.Dtos;
using triage_backend.Repositories;
using triage_backend.Utilities;

namespace triage_backend.Services
{
    public class PatientService : IPatientService
    {
        private readonly PatientRepository _patientRepository;

        public PatientService(PatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public object CreatePatient(PatientDto patientDto)
        {
            // ===============================
            // VALIDACIONES
            // ===============================

            if (string.IsNullOrWhiteSpace(patientDto.DocumentIdPt))
                return new { Success = false, Message = "El número de documento es obligatorio." };

            if (!Regex.IsMatch(patientDto.DocumentIdPt, @"^[0-9]+$"))
                return new { Success = false, Message = "El número de documento solo puede contener números." };

            if (string.IsNullOrWhiteSpace(patientDto.FirstNamePt))
                return new { Success = false, Message = "El nombre es obligatorio." };

            if (!Regex.IsMatch(patientDto.FirstNamePt, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
                return new { Success = false, Message = "El nombre solo puede contener letras." };

            if (string.IsNullOrWhiteSpace(patientDto.LastNamePt))
                return new { Success = false, Message = "El apellido es obligatorio." };

            if (!Regex.IsMatch(patientDto.LastNamePt, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
                return new { Success = false, Message = "El apellido solo puede contener letras." };

            if (string.IsNullOrWhiteSpace(patientDto.EmailPt))
                return new { Success = false, Message = "El correo electrónico es obligatorio." };

            if (!patientDto.EmailPt.Contains("@") || !patientDto.EmailPt.Contains("."))
                return new { Success = false, Message = "El correo electrónico no es válido." };

            if (patientDto.BirthDatePt == default)
                return new { Success = false, Message = "La fecha de nacimiento es obligatoria." };

            if (patientDto.BirthDatePt > DateTime.Now)
                return new { Success = false, Message = "La fecha de nacimiento no puede ser en el futuro." };

            // ✅ Género ya viene como bool → no requiere validación extra
            // false = Femenino, true = Masculino

            // ===============================
            // VALIDAR DUPLICADOS EN BD
            // ===============================
            bool exists = _patientRepository.ExistsByIdentificationOrEmail(patientDto.DocumentIdPt, patientDto.EmailPt);
            if (exists)
            {
                return new
                {
                    Success = false,
                    Message = "La identificación o el correo ya están registrados."
                };
            }

            // ===============================
            // ENCRIPTAR CONTRASEÑA (cédula como base)
            // ===============================
            string passwordHash = EncryptUtility.HashPassword(patientDto.DocumentIdPt);

            // ===============================
            // CREAR PACIENTE
            // ===============================
            int newId = _patientRepository.CreatePatient(patientDto, passwordHash);

            return new
            {
                Success = true,
                Message = "Paciente registrado exitosamente.",
                PatientId = newId
            };
        }
    }
}
