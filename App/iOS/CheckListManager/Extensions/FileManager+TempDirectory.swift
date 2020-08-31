//
//  FileManager+TempDirectory.swift
//  Checklyst
//
//  Created by Simone Testa on 28/08/2019.
//  Copyright © 2019 Johnson&Johnson. All rights reserved.
//

import Foundation

extension FileManager {
    
    func clearTemporaryDirectory() {
        do {
            let tmpDirURL = FileManager.default.temporaryDirectory
            let tmpDirectory = try contentsOfDirectory(atPath: tmpDirURL.path)
            try tmpDirectory.forEach { file in
                let fileUrl = tmpDirURL.appendingPathComponent(file)
                try removeItem(atPath: fileUrl.path)
            }
        } catch {
            print("Error while cleaning Temp Directory")
        }
    }
    
    func deleteFileinTemporaryDirectory(atURL url : URL) {
        do {
            let tmpDirURL = FileManager.default.temporaryDirectory
            let tmpDirectory = try contentsOfDirectory(atPath: tmpDirURL.path)
            try tmpDirectory.forEach { file in
                let fileUrl = tmpDirURL.appendingPathComponent(file)
                if fileUrl == url {
                    try removeItem(atPath: fileUrl.path)
                }
            }
        } catch {
            print("Error while remove \(url.absoluteString) in Temp Directory")
        }
    }
}
