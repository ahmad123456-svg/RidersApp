namespace RidersApp.ViewModels
{
    public class ConfigurationVM
    {
        public int ConfigurationId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string KeyName { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.StringLength(2000)]
        public string Value { get; set; } = string.Empty;
    }
}
