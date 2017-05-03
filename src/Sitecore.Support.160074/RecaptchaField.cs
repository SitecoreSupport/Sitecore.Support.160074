using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Form.Core.Data;
using Sitecore.Form.Web.UI.Controls;
using Sitecore.Forms.Mvc.Attributes;
using Sitecore.Forms.Mvc.Interfaces;
using Sitecore.Forms.Mvc.ViewModels.Fields;
using Sitecore.WFFM.Abstractions.Actions;
using Sitecore.WFFM.Abstractions.Dependencies;
using Sitecore.WFFM.Abstractions.Shared;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sitecore.Support.Forms.Mvc.ViewModels.Fields
{
  public class RecaptchaField : SingleLineTextField, IConfiguration, IValidatableObject
  {
    private readonly IAnalyticsTracker analyticsTracker;

    public RecaptchaField() : this(DependenciesManager.AnalyticsTracker)
    {
    }

    public RecaptchaField(IAnalyticsTracker analyticsTracker)
    {
      this.analyticsTracker = analyticsTracker;
      this.Theme = "light";
      this.CaptchaType = "image";
    }

    public virtual string GetAppSetting(string key)
    {
      Assert.ArgumentNotNullOrEmpty(key, "key");
      return System.Configuration.ConfigurationManager.AppSettings[key];
    }

    public override ControlResult GetResult() =>
        new ControlResult(base.FieldItemId, this.Title, this.Value, null, true);

    public virtual string GetSitecoreSetting(string key, string defaultValue)
    {
      Assert.ArgumentNotNullOrEmpty(key, "key");
      return Sitecore.Configuration.Settings.GetSetting(key, defaultValue);
    }

    public override void Initialize()
    {
      this.SiteKey = this.GetAppSetting("RecaptchaPublicKey") ?? this.GetSitecoreSetting("WFM.RecaptchaSiteKey", null);
      this.SecretKey = this.GetAppSetting("RecaptchaPrivateKey") ?? this.GetSitecoreSetting("WFM.RecaptchaSecretKey", null);
      base.Visible = (this.RobotDetection == null) || !this.RobotDetection.Enabled;
    }

    public override void SetValueFromQuery(string valueFromQuery)
    {
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      if ((!base.Visible && (this.RobotDetection != null)) && this.RobotDetection.Enabled)
      {
        ID formId = new ID(base.FormId);
        if ((this.RobotDetection != null) && this.RobotDetection.Session.Enabled)
        {
          this.RobotDetection.AddSubmitToSession(formId);
        }
        if ((this.RobotDetection != null) && this.RobotDetection.Server.Enabled)
        {
          this.RobotDetection.AddSubmitToServer(formId);
        }
        bool isRobot = this.IsRobot;
        bool flag2 = false;
        bool flag3 = false;
        if (this.RobotDetection.Session.Enabled)
        {
          flag2 = this.RobotDetection.IsSessionThresholdExceeded(formId);
        }
        if (this.RobotDetection.Server.Enabled)
        {
          flag3 = this.RobotDetection.IsServerThresholdExceeded(formId);
        }
        if ((isRobot || flag2) || flag3)
        {
          base.Visible = true;
          return new ValidationResult[] { new ValidationResult("You've been treated as a robot. Please enter the captcha to proceed") };
        }
      }
      return new ValidationResult[] { ValidationResult.Success };
    }

    public string CaptchaType { get; set; }

    public virtual bool IsRobot =>
        this.analyticsTracker.IsRobot;

    [TypeConverter(typeof(ProtectionSchemaAdapter))]
    public virtual ProtectionSchema RobotDetection { get; set; }

    public string SecretKey { get; set; }

    public string SiteKey { get; set; }

    public string Theme { get; set; }

    [RequestFormValue("g-recaptcha-response"), Sitecore.Support.Forms.Mvc.Validators.RecaptchaResponseValidator(ParameterName = "RecaptchaValidatorError")]
    public override string Value { get; set; }
  }
}
