 // API-adress (ändra till min egen)
    const API_BASE = "https://visitorapp-e8bef2g9dgdgfpcg.swedencentral-01.azurewebsites.net/api/checkin?code=radjW0VerNjBxVU7M3ZDDwBsUFlZ-jjSy1xS73GfFRxnAzFuzLW9wA==";
    
    // Hämta formulär + meddelande
    const form = document.getElementById('checkin-form');
    const msg = document.getElementById('msg');

    // När man skickar formuläret via funktion
    form.addEventListener('submit', async (e) => {
      e.preventDefault();
      msg.textContent = "";
      msg.className = "";
      
      // Plocka värden från fälten
      const payload = {
        name: document.getElementById('name').value.trim(),
        email: document.getElementById('email').value.trim() || null
      };

      // Kolla namn
      if (!payload.name) {
        msg.textContent = "Please enter your name.";
        msg.className = "err";
        return;
      }

      try {
        // Skicka till API
        const res = await fetch(API_BASE, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload)
        });

        // OK-svar
        if (res.ok) {
          msg.textContent = "✅ Checked in — thank you!";
          msg.className = "ok";
          form.reset();
        } else {
          // Fel från server
          const text = await res.text();
          msg.textContent = "❌ Failed: " + text;
          msg.className = "err";
        }
      } catch (err) {
        // Nätverksfel
        msg.textContent = "❌ Network error. Check the API URL and CORS settings.";
        msg.className = "err";
      }
    });