using System.Runtime.InteropServices;

namespace Core;

internal class TZICrypt
{
    /// <summary>
    /// belt-block: зашифрование блока
    /// </summary>
    /// <param name="x">[in] – указатель на сообщение для зашифрования</param>
    /// <param name="x_size">[in] – размер передаваемого сообщения</param>
    /// <param name="k"[in] – указатель на ключ (32 байта)></param>
    /// <param name="y">[out] – указатель на полученное сообщение (x_size байт)</param>
    /// <returns></returns>
    [DllImport("TZICrypt.dll")]
    internal static extern Err_type tzi_belt_ecb_encr(byte[] x, uint x_size, byte[] k, byte[] y);

    /// <summary>
    /// belt-block: расшифрование блока
    /// </summary>
    /// <param name="x">[in] – указатель на сообщение для расшифрования</param>
    /// <param name="x_size">[in] – размер передаваемого сообщения</param>
    /// <param name="k"[in] – указатель на ключ (32 байта)></param>
    /// <param name="y">[out] – указатель на полученное сообщение (x_size байт)</param>
    /// <returns></returns>
    [DllImport("TZICrypt.dll")]
    internal static extern Err_type tzi_belt_ecb_decr(byte[] x, uint x_size, byte[] k, byte[] y);

    // belt-hash: хэширование (СТБ 34.101.31, п. 7.8.3)
    /// <summary>
    /// belt-hash: хэширование
    /// </summary>
    /// <param name="x">[in] – указатель на сообщение</param>
    /// <param name="x_size">[in] – размер сообщения</param>
    /// <param name="y">[out] – указатель на результирующее хэш-значение (32 байта)</param>
    /// <returns></returns>
    [DllImport("TZICrypt.dll")]
    internal static extern Err_type tzi_belt_hash(byte[] x, uint x_size, byte[] y);
}

/// <summary>
/// Тип ошибки (из DLL) 
/// </summary>
internal enum Err_type
{
    TZI_OK = 0,                       // Успешное выполнение
    TZI_ERROR_INVALID_PARAM,          // Неверный параметр
    TZI_ERROR_INVALID_SIZE,           // Неверный размер данных
    TZI_ERROR_INVALID_STATE,          // Неверное состояние
    TZI_ERROR_BUFFER_TOO_SMALL,       // Буфер слишком мал
    TZI_ERROR_VERIFICATION_FAILED,    // Ошибка проверки
    TZI_ERROR_INTERNAL                // Внутренняя ошибка
}