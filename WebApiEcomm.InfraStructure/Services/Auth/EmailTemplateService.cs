namespace WebApiEcomm.InfraStructure.Services.Auth
{
    public class EmailTemplateService
    {
        public (string Subject, string HtmlBody) BuildVerificationTemplate(string userName, string code)
        {
            var subject = "Your verification code";
            var safeUserName = System.Net.WebUtility.HtmlEncode(userName);
            var safeCode = System.Net.WebUtility.HtmlEncode(code);
            var displayCode = string.Join(" ", safeCode.ToCharArray());

            var body = $@"
<!doctype html>
<html lang='en'>
  <head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{subject}</title>
  </head>
  <body style='margin:0; padding:0; background:#f2f2f2; font-family:Arial, Helvetica, sans-serif; color:#111827;'>
    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='background:#f2f2f2; margin:0; padding:14px 0;'>
      <tr>
        <td align='center'>
          <table role='presentation' width='476' cellspacing='0' cellpadding='0' style='width:476px; max-width:100%; background:#ffffff; border-collapse:collapse;'>
            <tr>
              <td align='center' style='padding:28px 24px 20px 24px; border-bottom:1px solid #dddddd;'>
                <h1 style='margin:0; font-size:21px; line-height:28px; font-weight:700; color:#1f2937;'>Ecommerce App</h1>
              </td>
            </tr>

            <tr>
              <td style='padding:27px 25px 8px 25px;'>
                <p style='margin:0 0 19px 0; font-size:14px; line-height:22px; color:#000000;'>Hello {safeUserName},</p>
                <p style='margin:0; font-size:14px; line-height:21px; color:#000000;'>
                  We received a request to verify your email for your account.
                  Please use the code below to complete your registration:
                </p>
              </td>
            </tr>

            <tr>
              <td align='center' style='padding:22px 25px 25px 25px;'>
                <table role='presentation' cellspacing='0' cellpadding='0' style='border-collapse:separate; border-spacing:0;'>
                  <tr>
                    <td align='center' style='min-width:152px; padding:21px 26px; border:1px solid #d9dee6; border-radius:5px; background:#f9fafb;'>
                      <span style='font-size:28px; line-height:34px; font-weight:700; letter-spacing:7px; color:#1f2937;'>{displayCode}</span>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>

            <tr>
              <td style='padding:16px 25px 26px 25px;'>
                <p style='margin:0 0 18px 0; font-size:12px; line-height:20px; color:#111827;'>This verification code will expire in 15 minutes.</p>
                <p style='margin:0; font-size:14px; line-height:21px; color:#000000;'>
                  If you did not create a Lost &amp; Found account, you can safely ignore this email.
                </p>
              </td>
            </tr>

            <tr>
              <td style='padding:26px 25px 25px 25px; background:#f8f9fb; border-top:1px solid #dddddd; border-bottom:1px solid #dddddd;'>
                <p style='margin:0 0 14px 0; font-size:12px; line-height:19px; color:#374151;'>
                  You received this email because an email verification was requested for your Lost &amp; Found account.
                </p>
                <p style='margin:0; font-size:12px; line-height:19px; color:#374151;'>
                  If this was not you, please contact us immediately.
                </p>
              </td>
            </tr>

            <tr>
              <td style='padding:14px 25px 18px 25px; background:#f8f9fb;'>
                <p style='margin:0 0 5px 0; font-size:10px; line-height:15px; color:#6b7280;'>Lost &amp; Found App</p>
                <p style='margin:0 0 5px 0; font-size:10px; line-height:15px; color:#6b7280;'>
                  Contact: <a href='mailto:mh1191128@gmail.com' style='color:#0b77d5; text-decoration:none;'>mh1191128@gmail.com</a>
                </p>
                <p style='margin:0; font-size:10px; line-height:15px; color:#6b7280;'>&copy; 2026 . All rights reserved.</p>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>";
            return (subject, body);
        }
    }
}
