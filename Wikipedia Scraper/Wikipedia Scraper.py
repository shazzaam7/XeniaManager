import requests
from bs4 import BeautifulSoup
import json

# Wikipedia URL with the Xbox 360 titles table
#url = "https://en.wikipedia.org/wiki/List_of_Xbox_360_games_(A%E2%80%93L)"
url = "https://en.wikipedia.org/wiki/List_of_Xbox_360_games_(M%E2%80%93Z)"
# Function to extract image URL from the title's Wikipedia page
def extract_image_url(title_link):
    response = requests.get("https://en.wikipedia.org" + title_link)
    if response.status_code == 200:
        soup = BeautifulSoup(response.content, 'html.parser')
        # Find the main image of the article
        image = soup.find('img', {'class': 'mw-file-element'})
        if image:
            image_url = image.get('src')
            # Prepend "https://" to the image URL if it doesn't start with it already
            if not image_url.startswith("https://"):
                image_url = "https:" + image_url
            print(image_url)
            return image_url
    return None



# Send a GET request to the URL
response = requests.get(url)

# Check if the request was successful
if response.status_code == 200:
    # Parse the HTML content
    soup = BeautifulSoup(response.content, 'html.parser')
    
    # Find the table containing Xbox 360 titles
    table = soup.find('table', {'class': 'wikitable sortable'})
    
    # Initialize an empty list to store the table data
    xbox_360_titles = []

    # Extract table rows
    rows = table.find_all('tr')

    # Iterate over each row (skipping the header row)
    for row in rows[1:]:
        # Extract table cells (columns)
        cells = row.find_all(['td', 'th'])
        # Extract data from each cell and remove any unnecessary characters
        title = cells[0].text.strip()
        print(title)
        # Extract the hyperlink and its URL
        link = cells[0].find('a')
        if link:
            game_link = link.get('href')
            image_url = extract_image_url(game_link)
        else:
            game_link = None
            image_url = None
        developer = cells[1].text.strip()
        publisher = cells[2].text.strip()
        genre = cells[3].text.strip()
        #release_date = cells[4].text.strip()
        
        # Create a dictionary for the current row
        game_data = {
            "Title": title,
            "Link": game_link,
            "Image URL": image_url,
            "Developer": developer,
            "Publisher": publisher,
            "Genre": genre
        }
        
        # Append the dictionary to the list
        xbox_360_titles.append(game_data)

    # Save the data as .json
    with open('xbox_360_titles.json', 'w') as json_file:
        json.dump(xbox_360_titles, json_file, indent=4)

    print("Data scraped and saved successfully as xbox_360_titles.json!")
else:
    print("Failed to retrieve data from Wikipedia.")
