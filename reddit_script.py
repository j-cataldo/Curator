import urllib2
import json
import os, sys

def downloadPostsFrom( subreddit ):
	try:
		f = urllib2.urlopen( 'http://www.reddit.com/r/' + subreddit + '/.json' )
		return json.loads( f.read().decode( 'utf-8' ) )["data"]["children"]
		
	except Exception:
		downloadPostsFrom( subreddit )
		#print( "ERROR: malformed JSON response from reddit.com" )
		#return ValueError
		
def parseImageURL_From( posts ):
	for node in posts:
		post = node['data']
		if post['domain'].endswith( 'imgur.com' ):
			yield post['url'], post['title']
			
def makeFileExt( content_type ):
	return {
		'image/bmp' : 'bmp',
		'image/gif' : 'gif',
		'image/jpeg' : 'jpg',
		'image/png' : 'png',
		'image/tiff' : 'tiff',
		'image/x-icon' : 'ico'
		}.get( content_type, 'txt' )
		
def makeFileName( title ):
	title = title.strip( ' .?:' )
	#file_name = title.translate( ''.maketrans( '\/:*?"<>|$@.','__.-_____S0___' ) )
	#file_name = file_name.strip( '_' ).replace( '__', '_' ).lower()
	file_name = title.strip( '_' ).replace( '__', '_' ).lower()
	return file_name + '.' if file_name else 'default.'
	
def makeSaveDir( save_dir ):
	if not os.path.exists( save_dir ):
		os.makedirs( save_dir )
		return save_dir
		
def downloadImagesIntoDir( save_dir, image_refs, save_path ):
	
	#save_dir = makeSaveDir( save_dir )
	save_dir = save_path + '/' + save_dir
	#print save_dir
	makeSaveDir( save_dir )
	#print save_dir
	#print( 'saving to:', save_dir )
	#print 'here'
	for url, title in image_refs:	
		try:
			request = urllib2.urlopen( url, timeout = 1 )
			if int(request.code) == 200:
				content_type = request.headers['Content-Type']
				if 'image' == content_type.split( '/' )[0]:
					#print content_type
					file_name = save_dir + '/' +  makeFileName( title ) + makeFileExt( content_type )
					print 'saving to:', file_name
					print( '  downloading:', title )
					with open( file_name, "wb" ) as image_file:
						image_file.write( request.read() )
		except Exception:
			continue
			#print ( "ERROR: bad request --", title )
			
def downloadRedditImages( subreddits, save_path ):
	for subreddit in subreddits:
		try:
			reddit_posts = downloadPostsFrom( subreddit )
			image_urls = parseImageURL_From( reddit_posts )		
			downloadImagesIntoDir( subreddit, image_urls, save_path )
		except Exception:
			print Exception
			
def main():
	if len( sys.argv ) > 2:
		downloadRedditImages( sys.argv[2:], sys.argv[1] )
		
if __name__ == '__main__':
	main()