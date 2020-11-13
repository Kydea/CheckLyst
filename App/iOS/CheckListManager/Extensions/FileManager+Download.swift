//
//  FileManager+Download.swift
//  Checklyst
//
//  Created by Simone Testa on 28/08/2019.
//  Copyright © 2019 Johnson&Johnson. All rights reserved.
//

import Foundation

extension FileManager {
    
    func downloadFile(_ url : String, completition : @escaping ((_ fileUrl : URL?,_ error : NSError?, _ data : Data? ) -> Void)){
        
        let filename = URL(string: url)?.queryParameters?["key"]?.replacingOccurrences(of: "/", with: "")
        let tempDirectoryURL = FileManager.default.temporaryDirectory
        let targetURL = tempDirectoryURL.appendingPathComponent(filename != nil ? filename! : "tempFile")
        
        ServerConnection.getFile(url) { (serverResponse) -> Void in
            if(serverResponse.data != nil) {
                do {
                    try serverResponse.data!.write(to: targetURL, options: .atomic)
                    completition(targetURL, nil, serverResponse.data)
                } catch let error {
                    NSLog("Unable to copy file: \(error)")
                    completition(nil, nil,nil)
                }
            } else {
                completition(nil, serverResponse.error, nil)
            }
        }
        //Meccanismo di cache sospeso, va raffinato
        /*if FileManager.default.fileExists(atPath: targetURL.path), let modificationDate =
            FileManager.default.getFileModificationDate(atPath: targetURL.path), modificationDate.timeIntervalSince(Date()) > -60 {
            let data = FileManager.default.contents(atPath: targetURL.path)
            completition(targetURL, nil, data)
            
        } else {
            ServerConnection.getFile(url) { (serverResponse) -> Void in
                if(serverResponse.data != nil) {
                    do {
                        try serverResponse.data!.write(to: targetURL, options: .atomic)
                        completition(targetURL, nil, serverResponse.data)
                    } catch let error {
                        NSLog("Unable to copy file: \(error)")
                        completition(nil, nil,nil)
                    }
                } else {
                    completition(nil, serverResponse.error, nil)
                }
            }
        }*/
    }
}
