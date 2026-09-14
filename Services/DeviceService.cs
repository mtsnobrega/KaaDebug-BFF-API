/*
 * Responsabilidade:
 * Verifica o status e a disponibilidade de um dispositivo IoT (ESP32) 
 * com base em seu código de fábrica.
 *
 * Papel na arquitetura:
 * Intermediação simples. Formata a resposta para evitar que a entidade 'Device'
 * (que contém chaves estrangeiras) para a API pública. Utiliza a regra de negócio 
 * que checa se o dispositivo já possui uma planta (Plant == null) para definir 
 * dinamicamente se está "UNASSOCIATED".
 */

using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Repositories.Interfaces;
using kaadebug_bff_api.Services.Interfaces;

namespace kaadebug_bff_api.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository _deviceRepo;

        public DeviceService(IDeviceRepository deviceRepo)
        {
            _deviceRepo = deviceRepo;
        }

        public async Task<ServiceResult<DeviceVerificationResponse>> VerifyAsync(string deviceCode)
        {
            var device = await _deviceRepo.GetByCodeAsync(deviceCode);

            if (device is null)
                return ServiceResult<DeviceVerificationResponse>.NotFound("Dispositivo não encontrado.");

            string statusToReturn;

            if (device.Plant == null)
            {
                statusToReturn = "UNASSOCIATED";
            }
            else
            {
                statusToReturn = device.ConnectionStatus.ToString().ToUpper();
            }

            var response = new DeviceVerificationResponse(
                device.Code,
                statusToReturn,
                device.LastHeartbeatAt);

            return ServiceResult<DeviceVerificationResponse>.Ok(response);
        }
    }
}
