using Newtonsoft.Json;
using Sitecore.Diagnostics;
using Sitecore.Forms.Mvc.Interfaces;
using Sitecore.Forms.Mvc.Validators;
using Sitecore.Forms.Mvc.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net;

namespace Sitecore.Support.Forms.Mvc.Validators
{
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false), DisplayName("TITLE_ERROR_MESSAGE_RECAPTCHA")]
  public class RecaptchaResponseValidator : DynamicValidationBase
  {
    protected override ValidationResult ValidateFieldValue(IViewModel model, object value, ValidationContext validationContext)
    {
      Assert.IsNotNull(model, "model");
      if (value == null)
      {
        return new ValidationResult(this.FormatError(model, new object[] { "Invalid captcha text" }));
      }
      WebClient client = new WebClient();
      ReCaptchaResponse response = JsonConvert.DeserializeObject<ReCaptchaResponse>(client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", ((Sitecore.Support.Forms.Mvc.ViewModels.Fields.RecaptchaField)model).SecretKey, value)));
      if (response.Success)
      {
        return ValidationResult.Success;
      }
      return new ValidationResult(this.FormatError(model, response.ErrorCodes.ToArray()));
    }

    public override string GetErrorMessageTemplate(object fieldModel)
    {
      string key = (string.IsNullOrEmpty(this.ParameterName) ? base.GetType().Name : this.ParameterName).ToLowerInvariant();

      FieldViewModel fieldViewModel = (FieldViewModel)fieldModel;
      if (fieldViewModel != null)
      {
        Dictionary<string, string> parameters = fieldViewModel.Parameters;
        if (parameters.ContainsKey(key))
        {
          return parameters[key];
        }
      }

      return base.GetErrorMessageTemplate(fieldModel);
    }

    protected override string FormatError(object model, params object[] parameters)
    {
      Assert.ArgumentNotNull(model, "model");
      FieldViewModel fieldViewModel = model as FieldViewModel;
      string text = string.Empty;
      if (fieldViewModel != null)
      {
        string errorMessageTemplate = this.GetErrorMessageTemplate(fieldViewModel);
        text = fieldViewModel.Title;
        if (!parameters.Any<object>())
        {
          return string.Format(CultureInfo.CurrentCulture, errorMessageTemplate, new object[]
          {
            text
          });
        }
      }
      List<object> list = new List<object>
      {
        text
      };
      list.AddRange(parameters);

      string str = string.Format(CultureInfo.CurrentCulture, this.GetErrorMessageTemplate(fieldViewModel), list.ToArray());

      return str;
    }
  }
}
