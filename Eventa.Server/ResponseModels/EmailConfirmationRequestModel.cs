﻿namespace Eventa.Server.ResponseModels
{
    public class EmailConfirmationRequestModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
