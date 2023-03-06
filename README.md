# BazosBot

Bot for the marketplace portal Bazoš, it operates in several countries in central Europe.

# Links
- [Bazos.cz](https://bazos.cz)
- [Bazos.sk](https://bazos.sk)
- [Bazos.pl](https://bazos.pl)
- [Bazos.at](https://bazos.at)

**The bot supports all above mentioned locations!**

# Listing restorer
With each run the bot saves all available listings locally. Sometimes Bazos can hard-delete your listings, making them impossible to recover easily (only manually). By enabling restorer in the configuration file, the bot checks for deleted listings and asks you if you want to restore particular listings.

- Bazoš started to ban listing restorer's saved images using their cryptographic fingerprint. **Before each run, the bot is going to ask you, whether to try and circumvent this obstacle. This means changing a random pixel within the size of the picture to white.** 

# Intent

Listings on Bazoš are sorted by the date of creation. The site allows you to put the listings to the top by paying. BazosBot bypasses this functionality by extracting the data from your listings, removing them and then adding them back.

# Usage
- Compile
- Run
- Edit Files/<BAZOS-LOCATION>/Config.json
    - Input name, phone number & password associated with your listings
    - Input the "bid" and "bkod" cookies from your browser after verifying your phone number
    - Edit the number of days after which listings should renew, default is 2 
- Press any key to reload the configuration file!

# FOR EDUCATIONAL PURPOSES ONLY