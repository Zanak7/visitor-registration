 // API-adress (ändra till min egen)
    const API_BASE = "https://visitorapp.azurewebsites.net";
    //dpifhqduoh
    const form = document.getElementById('checkin-form');
    const msg = document.getElementById('msg');

    form.addEventListener('submit', async (e) => {
      e.preventDefault();
      msg.textContent = "";
      msg.className = "";
      //dijqhfkh
      const payload = {
        name: document.getElementById('name').value.trim(),
        email: document.getElementById('email').value.trim() || null
      };

      if (!payload.name) {
        msg.textContent = "Please enter your name.";
        msg.className = "err";
        return;
      }

      try {
        const res = await fetch(API_BASE + "/api/checkin", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload)
        });

        if (res.ok) {
          msg.textContent = "✅ Checked in — thank you!";
          msg.className = "ok";
          form.reset();
        } else {
          const text = await res.text();
          msg.textContent = "❌ Failed: " + text;
          msg.className = "err";
        }
      } catch (err) {
        msg.textContent = "❌ Network error. Check the API URL and CORS settings.";
        msg.className = "err";
      }
    });