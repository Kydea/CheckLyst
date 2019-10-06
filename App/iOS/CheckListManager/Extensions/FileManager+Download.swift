//
//  FileManager+Download.swift
//  Checklyst
//
//  Created by Simone Testa on 28/08/2019.
//  Copyright © 2019 Johnson&Johnson. All rights reserved.
//

import Foundation

extension FileManager {
    
    func getFile(_ url : String, completition : @escaping ((_ fileUrl : URL?,_ error : NSError? ) -> Void)){
        ServerConnection.getFile(url) { (serverResponse) -> Void in
            if(serverResponse.data != nil) {
                let filename = URL(string: url)?.queryParameters?["key"]?.replacingOccurrences(of: "/", with: "")
                
                let tempDirectoryURL = FileManager.default.temporaryDirectory
                let targetURL = tempDirectoryURL.appendingPathComponent(filename != nil ? filename! : "tempFile")
                do {
                    try serverResponse.data!.write(to: targetURL, options: .atomic)
                    completition(targetURL, nil)
                } catch let error {
                    NSLog("Unable to copy file: \(error)")
                    completition(nil, nil)
                }
            } else {
                completition(nil, serverResponse.error)
            }
        }
    }
}
