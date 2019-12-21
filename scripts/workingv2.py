import socket
import sys
import base64
import thread
import steps

max_data=1024
hasHB=False
		
def connect_hb():
	hb_sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
	server_address = ('37.187.151.148', 45067)
	print '[HB] connecting to %s port %s' % server_address
	hb_sock.connect(server_address)
	return hb_sock

def sendToHB(data,hb_sock):
	hb_sock.sendall(data)
	
def getFromHB(hb_sock):
	try:
		hb_sock.settimeout(1.0)
		data = hb_sock.recv(max_data)
		print '[+] server->client "%s" len:%s' % (base64.b64encode(data)[:10],len(data))
		return data
	except:
		return None

def workWithClient(connection, client_address,hb_sock):
	#try:
		i=0
		print '[+] ping', client_address
		while True:
			print '[+] i:%s'%(i)
			#connection.settimeout(1.0)
			data = connection.recv(max_data)
			if len(data)>1:
				print '[+] client->server "%s" len:%s' % (base64.b64encode(data)[:10],len(data))
			global hasHB
			if not hasHB:
				hasHB=True
				print '[!] first time HB'
				sendToHB(data,hb_sock)
				d=''
				data_=getFromHB(hb_sock)
				while(data_):
					d+=data_
					data_=getFromHB(hb_sock)
					if not data_:
						break
				connection.sendall(d)
			else:
				if i==0 and len(data)==71:
					print '[!] step 0'
					connection.sendall(base64.b64decode(steps.STEP_0))
				elif i==1 and len(data)==45:
					print '[!] step 1'
					connection.sendall(base64.b64decode(steps.STEP_1))
				elif i>1 and len(data)==13:
					print '[!] step ping_alive'
					connection.sendall(base64.b64decode(steps.STEP_PING))
				elif len(data) > 0:
					print '[!] step ?:%s'%(i)
					sendToHB(data,hb_sock)
					d=''
					data_=getFromHB(hb_sock)
					while(data_):
						d+=data_
						data_=getFromHB(hb_sock)
						if not data_:
							break
					connection.sendall(d)
				else:
					connection.close()
					sys.exit()
			i+=1
	#except:
	#	connection.close()
	#sys.exit()
		
def main():
	sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
	server_address = ('localhost', 45067)
	print '[+] starting up on %s port %s' % server_address
	sock.bind(server_address)
	sock.listen(10)
	hb_sock=connect_hb()
	while True:
		print '[+] ready'
		connection, client_address = sock.accept()
		thread.start_new_thread(workWithClient,(connection, client_address,hb_sock,))
		#workWithClient(connection, client_address)
		
if __name__ == '__main__':
	main()