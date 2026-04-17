# Gmail SMTP Configuration for NotificationService

## 🔐 Problem: Gmail Authentication Failed

When you see this error:

```
MailKit.Security.AuthenticationException: 535: 5.7.8 Username and Password not accepted
```

It means **Gmail is rejecting your password**. This is because Gmail requires an **App Password**, not your regular Gmail password.

---

## ✅ Solution: Create Google App Password

### **Step 1: Enable 2-Factor Authentication**

1. Go to [myaccount.google.com/security](https://myaccount.google.com/security)
2. Click **"2-Step Verification"**
3. Follow the setup wizard
4. Verify your phone number

### **Step 2: Generate App Password**

1. Go to [myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords)
2. Select **App**: `Mail`
3. Select **Device**: `Windows Computer` (or your OS)
4. Click **Generate**
5. Google will show a 16-character password like: `abcd efgh ijkl mnop`

### **Step 3: Copy the App Password**

The password will appear in a popup. **Copy it exactly** (without spaces for docker-compose):

```
abcdefghijklmnop  (remove spaces)
```

### **Step 4: Update docker-compose.yml**

```yaml
notificationservice:
  environment:
    Smtp__Username: your-email@gmail.com
    Smtp__Password: abcdefghijklmnop # <-- Your 16-char App Password
    Smtp__FromEmail: your-email@gmail.com
    Smtp__FromName: SmartShip
```

### **Step 5: Restart Container**

```bash
docker-compose down
docker-compose up --build notificationservice
```

---

## 🔍 Troubleshooting

### **Check Logs**

```bash
docker-compose logs notificationservice
```

Look for:

```
SMTP Configuration - Host: smtp.gmail.com, Port: 587, Username: you***
Authenticating with username: your-email@gmail.com
OTP email sent successfully
```

### **Common Issues**

| Issue                          | Solution                                                  |
| ------------------------------ | --------------------------------------------------------- |
| **2FA not enabled**            | Enable 2-Step Verification first (see Step 1)             |
| **App Password not generated** | Use the exact 16-char password from Google                |
| **Still getting 535 error**    | Wait 5-10 minutes for Google to sync the new app password |
| **"Less secure apps" block**   | App Passwords bypass this - no action needed              |

---

## 📧 Alternative: Use Corporate Email

If you can't use Gmail, use your corporate email's SMTP:

**Outlook/Office 365:**

```yaml
Smtp__Host: smtp.office365.com
Smtp__Port: 587
Smtp__Username: your-email@company.com
Smtp__Password: your-office-password
```

**SendGrid:**

```yaml
Smtp__Host: smtp.sendgrid.net
Smtp__Port: 587
Smtp__Username: apikey
Smtp__Password: SG.your-sendgrid-key
```

---

## ✅ Verification Checklist

- [ ] 2-Step Verification enabled in Google Account
- [ ] App Password generated from [myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords)
- [ ] Exact 16-character password used (no spaces)
- [ ] `docker-compose.yml` updated with password
- [ ] Container restarted: `docker-compose up --build`
- [ ] Logs show `OTP email sent successfully`
- [ ] Test OTP email received at destination

---

## 📝 Example Configuration

**docker-compose.yml:**

```yaml
notificationservice:
  build:
    context: .
    dockerfile: Services/NotificationService/Dockerfile
  container_name: smartship-notification
  ports:
    - "5005:8080"
  environment:
    ASPNETCORE_ENVIRONMENT: Development
    RabbitMQ__HostName: rabbitmq
    Smtp__Host: smtp.gmail.com
    Smtp__Port: 587
    Smtp__Username: john.doe@gmail.com
    Smtp__Password: abcdefghijklmnop
    Smtp__FromEmail: noreply@smartship.com
    Smtp__FromName: SmartShip Notifications
  networks:
    - smartship-network
  depends_on:
    rabbitmq:
      condition: service_healthy
  restart: on-failure
```

---

## 🚀 Testing the Setup

### **Test 1: Check logs for configuration**

```bash
docker-compose logs notificationservice | grep "SMTP Configuration"
```

Expected:

```
SMTP Configuration - Host: smtp.gmail.com, Port: 587, Username: joh***
```

### **Test 2: Trigger OTP via API**

```bash
curl -X POST http://localhost:5045/api/auth/signup \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test User",
    "email": "test@example.com",
    "password": "TestPassword123"
  }'
```

### **Test 3: Check NotificationService logs**

```bash
docker-compose logs notificationservice
```

Should see:

```
Sending OTP email to: test@example.com
OTP email sent successfully to: test@example.com
```

---

## 📞 Support Links

- [Google App Passwords Help](https://support.google.com/accounts/answer/185833)
- [Gmail SMTP Settings](https://support.google.com/mail/answer/7126229)
- [MailKit Documentation](https://www.mimekit.net/docs/html/T_MailKit_Net_Smtp_SmtpClient.htm)
