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
                return ServiceResult<DeviceVerificationResponse>.NotFound(
                    "Dispositivo não encontrado. Verifique o código na etiqueta.");

            var response = new DeviceVerificationResponse(
                Code: device.Code,
                ConnectionStatus: device.ConnectionStatus.ToString().ToUpper(),
                LastHeartbeatAt: device.LastHeartbeatAt);

            return ServiceResult<DeviceVerificationResponse>.Ok(response);
        }
    }
}
