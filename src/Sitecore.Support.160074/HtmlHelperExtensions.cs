using Sitecore.Diagnostics;
using Sitecore.Form.Core.Data;
using Sitecore.Forms.Mvc.Interfaces;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Sitecore.Forms.Mvc.Html;

namespace Sitecore.Support.Forms.Mvc.Html
{
  public static class HtmlHelperExtensions
  {
    public static MvcHtmlString Recaptcha(this HtmlHelper helper, IViewModel model = null)
    {
      ViewModels.Fields.RecaptchaField field = (model ?? helper.ViewData.Model) as ViewModels.Fields.RecaptchaField;
      Assert.IsNotNull(field, "view");
      ProtectionSchema robotDetection = field.RobotDetection;
      StringBuilder builder = new StringBuilder();
      builder.Append(helper.OpenFormField(field, (robotDetection == null) || !robotDetection.Enabled));
      builder.Append(helper.Hidden("Value"));
      if (field.Visible)
      {
        TagBuilder builder2 = new TagBuilder("div");
        builder2.AddCssClass("g-recaptcha");
        builder2.MergeAttribute("data-sitekey", field.SiteKey);
        builder2.MergeAttribute("data-theme", field.Theme);
        builder2.MergeAttribute("data-type", field.CaptchaType);
        TagBuilder builder3 = new TagBuilder("script");
        builder3.MergeAttribute("src", "https://www.google.com/recaptcha/api.js");
        builder.Append(builder2);
        builder.Append(builder3);
      }
      builder.Append(helper.CloseFormField(field, true));
      return MvcHtmlString.Create(builder.ToString());
    }
  }
}
