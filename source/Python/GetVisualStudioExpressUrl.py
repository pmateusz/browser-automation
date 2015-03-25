import requests
import bs4

VS_FREE_PRODUCTS_OFFERS = 'https://www.visualstudio.com/en-us/products/free-developer-offers-vs.aspx'
VS_CLASS = 'visual-studio'


def get_web_page():
    r = requests.get(VS_FREE_PRODUCTS_OFFERS)
    r.raise_for_status()
    return bs4.BeautifulSoup(r.text)


def is_vs_banner(banner):
    return banner.has_attr('class') and (VS_CLASS == banner['class'] or VS_CLASS in banner['class'])


def get_vs_link(soup):
    apps_table = soup.find('table', 'apps-table')
    banners_row, links_row = apps_table.find_all('tr')

    banners_row = banners_row.find_all('td')
    links_row = links_row.find_all('td')

    vs_index_generator = (index for index, banner in enumerate(banners_row) if is_vs_banner(banner))
    vs_first_index = next(vs_index_generator)

    assert len(links_row) > vs_first_index, 'Forward link to Visual Studio not found below application banner'

    link = links_row[vs_first_index]
    link = link.find('div', 'link').find('a')
    return link.get('href')

if __name__ == '__main__':
    page = get_web_page()
    hyperlink = get_vs_link(page)
    print(hyperlink)