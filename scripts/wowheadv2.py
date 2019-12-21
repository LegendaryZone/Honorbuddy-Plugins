import requests
import sys
import re
import json
from Tkinter import Tk

found=[]
black_listed=[]
total=0

s=requests.session()
s.verify=False
#s.headers.update(head)
proxies = {
	'http': 'http://127.0.0.1:8888',
	'https': 'http://127.0.0.1:8888',
}
#s.proxies.update(proxies)
patches=[('Cata','?filter=166;4;0'),
('Legion','?filter=166;7;0'),
('MoP','?filter=166;5;0'),
('TBC','?filter=166;2;0'),
('WoD','?filter=166;6;0'),
('WotLK','?filter=166;3;0'),
('Normal','?filter=166;1;0')]

real_work=['http://www.wowhead.com/consumables','http://www.wowhead.com/trade-goods','http://www.wowhead.com/gems','http://www.wowhead.com/miscellaneous-items']

def parse(url,patch_name,group_name):
	r=s.get(url)
	items=re.search('var listviewitems = \[.*\];',r.content)
	if items:
		items= items.group(0).replace(';','').replace('firstseenpatch','"firstseenpatch"').replace('true','True').replace('frommerge','"frommerge"').replace('var listviewitems = ','').replace('cost','"cost"')
		items = eval(items)
		group_='group:%s`%s'%(group_name,patch_name)
		found.append(group_)
		#print '[+] found %s items'%(len(items))
		i=0
		global total
		for item in items:
			if item['id'] not in black_listed:
				new_item='i:%s'%(item['id'])
				found.append(new_item)
				total+=1
				i+=1
			#else:
			#	print '[-] black listed item'
		print '[+] added %s items'%(i)
	else:
		#print '[-] no items:%s_%s'%(group_name,patch_name)
		pass

def work_with_list():
	str1 = ','.join(found)
	r = Tk()
	r.withdraw()
	r.clipboard_clear()
	r.clipboard_append(str1)
	r.destroy()		

def getBlackList(url):
	r=s.get(url)
	items=re.search('var listviewitems = \[.*\];',r.content)
	if items:
		items= items.group(0).replace(';','').replace('firstseenpatch','"firstseenpatch"').replace('true','True').replace('frommerge','"frommerge"').replace('var listviewitems = ','').replace('cost','"cost"')
		items = eval(items)
		i=0
		for item in items:
			black_listed.append(item['id'])
			i+=1
		print '[+] added %s to blacklist'%(i)
	else:
		print '[-] blacklist'
		pass
	
def main():
	if len(sys.argv)>1:
		under_cats=[]
		base=sys.argv[1]
		if base[-1:] != '/':
			#base = base[:-1]
			base = base+'/'
		r=s.get(base)
		getBlackList(base+'?filter=186;2;0')
		#getBlackList(base+'?filter=2;2;0')
		items_weightingAllowed =re.search('<div class="filter-side-facet"><select name="type" id="filter-facet-type".*',r.content)
		if not items_weightingAllowed:
			print '[-] bad link'
			exit(1)
		items_weightingAllowed=items_weightingAllowed.group(0)
		values = re.findall('<option value="-?[0-9]*">[a-zA-Z &()]*</option>',items_weightingAllowed)
		for val in values:
			if 'relics' in val.lower():
				continue
			name = re.sub('.*">','',val)
			name = re.sub('</.*','',name)
			id = re.sub('.*value="','',val)
			id = re.sub('">.*','',id)
			under_cats.append((name,id))
		under_cats=list(set(under_cats))
		for uc in under_cats:
			cat=uc[0].lower()
			print '[+] doing:%s'%(uc[0])
			url= base+'type:%s'%(uc[1])
			for p in patches:
				patch_name,patch_url= p
				url_= url+patch_url
				#print '[+] added in patch:%s'%(patch_name)
				parse(url_,patch_name,uc[0])
		global total
		print '[+] total of %s'%(total)
		print '[!] done work doing list'
		work_with_list()
	else:
		for thing in real_work:
			under_cats=[]
			base=thing
			if base[-1:] != '/':
				#base = base[:-1]
				base = base+'/'
			r=s.get(base)
			getBlackList(base+'?filter=186;2;0')
			#getBlackList(base+'?filter=2;1;0')
			items_weightingAllowed =re.search('<div class="filter-side-facet"><select name="type" id="filter-facet-type".*',r.content)
			if not items_weightingAllowed:
				print '[-] bad link'
				exit(1)
			items_weightingAllowed=items_weightingAllowed.group(0)
			values = re.findall('<option value="-?[0-9]*">[a-zA-Z &()]*</option>',items_weightingAllowed)
			for val in values:
				val_=val.lower()
				if 'relics' in val_ or 'junk' in val_ or 'bandages' in val_ or 'holiday' in val_ or 'reagents' in val_ or 'parts' in val_ or 'other ' in val_ or 'consumables' in val_ or 'tokens' in val_ or 'item enha' in val_ or 'gems' in val_ and not 'prismatic' in val_:
					continue
				name = re.sub('.*">','',val)
				name = re.sub('</.*','',name)
				id = re.sub('.*value="','',val)
				id = re.sub('">.*','',id)
				under_cats.append((name,id))
			under_cats=list(set(under_cats))
			for uc in under_cats:
				cat=uc[0].lower()
				print '[+] doing:%s'%(uc[0])
				url= base+'type:%s'%(uc[1])
				for p in patches:
					patch_name,patch_url= p
					url_= url+patch_url
					#print '[+] added in patch:%s'%(patch_name)
					parse(url_,patch_name,uc[0])
			#global total
			#print '[+] total of %s'%(total)
			print '[!] done work doing list'
		global total
		print '[+] total of %s'%(total)
		work_with_list()
		
if __name__ == '__main__':
	main()
	
#group:herb`legion,i:124104,group:stone`legion,i:72092
#group:herb`legion,i:124104,group:stone`cata,i:124444,group:stone`legion,i:72092
'''
		if False:
			base=sys.argv[1]
			for p in patches:
				patch_name,patch_url= p
				parse(base+patch_url,patch_name)
			print 'done, now copy list'
			work_with_list()
		else:
'''